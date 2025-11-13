using System;
using UnityEngine;
using UnityEngine.Events;

public class SimulationController : MonoBehaviour
{
    [SerializeField] private SimulationModel model;
    [SerializeField] private bool isRunning = false;
    [SerializeField] private bool isPaused = false;

    [SerializeField] private float customerSpawnInterval = 2f;
    [SerializeField] private int minItems = 1;
    [SerializeField] private int maxItems = 20;
    private float timeSinceLastSpawn = 0f;
    private int customerCounter = 0;
    public UnityEvent OnCustomerServed;
    public UnityEvent OnCustomerLeft;
    public UnityEvent OnRegisterBroken;
    public UnityEvent OnSimulationStarted;
    public UnityEvent OnSimulationPaused;
    public UnityEvent OnSimulationReset;

    void Awake()
    {
        OnCustomerServed ??= new UnityEvent();
        OnCustomerLeft ??= new UnityEvent();
        OnRegisterBroken ??= new UnityEvent();
        OnSimulationStarted ??= new UnityEvent();
        OnSimulationPaused ??= new UnityEvent();
        OnSimulationReset ??= new UnityEvent();

        if (model == null)
        {
            model = GetComponent<SimulationModel>();
            if (model == null)
            {
                Debug.LogError("SimulationModel not found! Please assign it in the inspector.");
            }
        }
    }

    void Update()
    {
        if (isRunning && !isPaused)
        {
            Tick(Time.deltaTime);
        }
    }

    public void Tick(float deltaTime)
    {
        if (model == null) return;

        timeSinceLastSpawn += deltaTime;
        if (timeSinceLastSpawn >= customerSpawnInterval)
        {
            SpawnRandomCustomer();
            timeSinceLastSpawn = 0f;
        }

        CheckCustomerStatuses();

        CheckRegisterBreakdowns();
    }

    public void StartSimulation()
    {
        isRunning = true;
        isPaused = false;
        OnSimulationStarted?.Invoke();
        Debug.Log("Simulation started");
    }

    public void PauseSimulation()
    {
        isPaused = !isPaused;
        OnSimulationPaused?.Invoke();
        Debug.Log(isPaused ? "Simulation paused" : "Simulation resumed");
    }

    public void StopSimulation()
    {
        isRunning = false;
        isPaused = false;
        Debug.Log("Simulation stopped");
    }

    public void ResetSimulation()
    {
        if (model == null) return;

        isRunning = false;
        isPaused = false;
        timeSinceLastSpawn = 0f;
        customerCounter = 0;

        model.Customers.Clear();

        foreach (var register in model.Registers)
        {
            register.Customers.Clear();
        }

        OnSimulationReset?.Invoke();
        Debug.Log("Simulation reset");
    }

    private void SpawnRandomCustomer()
    {
        if (model == null) return;

        customerCounter++;
        string customerId = $"Customer_{customerCounter}";
        int itemCount = UnityEngine.Random.Range(minItems, maxItems + 1);

        model.SpawnCustomer(customerId, itemCount);
        Debug.Log($"Spawned {customerId} with {itemCount} items");
    }

    private void CheckCustomerStatuses()
    {
        if (model == null) return;

        for (int i = model.Customers.Count - 1; i >= 0; i--)
        {
            var customer = model.Customers[i];

            if (customer.AlreadyServed && customer.ServiceEndTime > 0)
            {
                OnCustomerServed?.Invoke();
                model.RemoveCustomer(customer);
                Debug.Log($"{customer.Id} was served. Wait time: {customer.GetTotalWaitTime():F2}s");
            }
            // TODO: Добавить логику для ушедших клентов
        }
    }

    private void CheckRegisterBreakdowns()
    {
        if (model == null) return;

        foreach (var register in model.Registers)
        {
            if (register.Status == RegisterStatus.Open &&
                UnityEngine.Random.value < register.BreakProbability * Time.deltaTime)
            {
                register.BreakDown();
                OnRegisterBroken?.Invoke();
                Debug.Log($"Register {register.Id} broke down!");
            }
        }
    }


    public void SetCustomerSpawnInterval(float interval)
    {
        customerSpawnInterval = Mathf.Max(0.1f, interval);
    }

    public void SetItemRange(int min, int max)
    {
        minItems = Mathf.Max(1, min);
        maxItems = Mathf.Max(minItems, max);
    }

    public void SetTimeScale(float scale)
    {
        model?.SetTimeScale(scale);
    }

    public void SetTimeScaleX1() => model?.SetTimeScaleX1();
    public void SetTimeScaleX2() => model?.SetTimeScaleX2();
    public void SetTimeScaleX5() => model?.SetTimeScaleX5();

    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public SimulationModel Model => model;
}
