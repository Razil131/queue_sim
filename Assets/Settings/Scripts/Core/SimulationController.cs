using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SimulationController : MonoBehaviour
{
    [SerializeField] private SimulationModel model;
    [SerializeField] private bool isRunning = false;
    [SerializeField] private bool isPaused = false;
    [SerializeField] private SimulationView view;

    [SerializeField] private float customerSpawnInterval = 4f;
    [SerializeField] private int minItems = 1;
    [SerializeField] private int maxItems = 20;
    [SerializeField] private bool isDeleteMode = false;
    [SerializeField] private int CheckoutMax = 32;
    private float timeSinceLastSpawn = 0f;
    private int customerCounter = 0;
    public UnityEvent OnCustomerServed;
    public UnityEvent OnCustomerLeft;
    public UnityEvent OnRegisterBroken;
    public UnityEvent OnSimulationStarted;
    public UnityEvent OnSimulationPaused;
    public UnityEvent OnSimulationReset;
    public bool IsDeleteMode => isDeleteMode;

    public SimulationView View => view;

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
        if (model != null)
        {
            model.IsPaused = isPaused;
        }

        if (isRunning && !isPaused)
        {
            Tick(Time.deltaTime);
            foreach (var register in model.Registers)
            {
                if(register is StaffedCashRegister staffedRegister)
                {
                    staffedRegister.SetProgress();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PauseSimulation();
        }
    }
    
    public void SpawnRegisters()
    {
        if (model == null || view == null || view.GridView == null)
        {
            Debug.LogError("Grid or View missing!");
            return;
        }

        int[,] positions = new int[,]
        {
            // { 0, 0 },
            { 20, 10 },
            // { 10, 2 }
        };

        Vector3[] queueDirections = new Vector3[]
        {
            // new Vector3(-1f, 0f, 0f),
            new Vector3(-1f, 0f, 0f),
            // new Vector3(-1f, 0f, 0f)
        };

        float registerWidthInCells = 2f;
        float registerHeightInCells = 1f;

        float cellSize = view.GridView.cellSize;

        //float offset = -0.5f * cellSize;

        for (int i = 0; i < positions.GetLength(0); i++)
        {
            int startX = positions[i, 0];
            int startY = positions[i, 1];

            var register = new StaffedCashRegister(
                $"Register_{i + 1}",
                QueueType.Normal,
                1f,
                0.01f
            );

            Vector3 basePos = view.GridView.GetRegisterSpawnPosition(startX, startY, registerWidthInCells, registerHeightInCells);

            register.Position = basePos;
            register.QueueDirection = queueDirections[i];

            model.AddRegister(register);

            Debug.Log($"✅ Register {register.Id} at GRID ({startX},{startY}) → WORLD {register.Position}, Queue Direction: {register.QueueDirection}");
        }

        view.RenderRegisters(model.Registers);
    }

    public void Tick(float deltaTime)
    {
        if (model == null) return;

        timeSinceLastSpawn += deltaTime*model.TimeScale;
        if (timeSinceLastSpawn >= customerSpawnInterval)
        {
            SpawnRandomCustomer();
            timeSinceLastSpawn = 0f;
        }

        CheckCustomerStatuses();

        CheckRegisterBreakdowns();
        view.RenderRegisters(model.Registers);
        view.RenderCustomers(model.Customers);
    }


    public void StartSimulation()
    {
        view.RenderGrid();
        SpawnRegisters();
        isRunning = true;
        isPaused = false;
        
        var ui = FindAnyObjectByType<UIController>();
        ui?.UpdateRegisterCountText();
        
        OnSimulationStarted?.Invoke();
        Debug.Log("Simulation started");
    }

    public void PauseSimulation()
    {
        isPaused = !isPaused;
        if (model != null)
        {
            model.IsPaused = isPaused;
        }
        
        OnSimulationPaused?.Invoke();
        Debug.Log(isPaused ? "Simulation PAUSED" : "Simulation RESUMED");
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

        foreach (var register in model.Registers)
        {
            register.Customers.Clear();
            if (register is StaffedCashRegister staffed)
            {
                staffed.ClearNowServing();
            }
            // else if (register is SelfCheckout selfCheckout)
            // {
            //     selfCheckout.ClearNowServing();
            // }
        }

        model.Customers.Clear();
        customerViews.Clear();

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
        customerSpawnInterval = Mathf.Max(0.1f, 60/interval);
        Debug.Log("interval changed");
    }

    public float GetCustomerSpawnInterval()
    {
        return customerSpawnInterval;
    }

    public void SetItemRange(int min, int max)
    {
        minItems = Mathf.Max(1, min);
        maxItems = Mathf.Max(minItems, max);
    }

    public void SetMinItems(int min)
    {
        minItems = Mathf.Max(1, min);
        maxItems = Mathf.Max(minItems, maxItems);
    }

    public void SetMaxItems(int max)
    {
        maxItems = Mathf.Max(minItems, max);
    }

    public int GetMinItems()
    {
        return minItems;
    }

    public int GetMaxItems()
    {
        return maxItems;
    }

    public void OnRegisterClicked(string registerId)
{
    ICashRegister selectedRegister = model.Registers.Find(r => r.Id == registerId);
    if (selectedRegister != null)
    {
        foreach (var reg in model.Registers)
        {
               reg.IsSelected = false; 
        }

        selectedRegister.IsSelected = true;
    }
} //TODO не используется, нуждается в доработке, дописать в диаграммы


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

    public void RequestManualCustomerSpawn()
    {
    SpawnRandomCustomer();
    }



    private List<CustomerView> customerViews = new List<CustomerView>();

    public void RemoveAllCustomers()
    {
        foreach (var view in customerViews)
        {
            if (view != null)
                Destroy(view.gameObject);
        }

        customerViews.Clear();
        model.Customers.Clear();

        foreach (var register in model.Registers)
        {
            register.Customers.Clear();
            if (register is StaffedCashRegister staffed)
            {
                staffed.ClearNowServing();
            }
            // else if (register is SelfCheckout selfCheckout)
            // {
            //     selfCheckout.ClearNowServing();
            // }
        }

        Debug.Log("All customers removed.");
    }

    public void RegisterCustomerView(CustomerView view)
    {
        if(!customerViews.Contains(view))
            customerViews.Add(view);
    }

    public void UnregisterCustomerView(CustomerView view)
    {
        if (customerViews.Contains(view))
            customerViews.Remove(view);
    }


    public void RemoveCustomerSafe(Customer customer)
{
    if (customer == null) return;

    if (customer.CurrentRegister != null)
    {
        if (customer.CurrentRegister is StaffedCashRegister staffed)
        {
            staffed.RemoveCustomerFromQueue(customer);
        }
        // else if (customer.CurrentRegister is SelfCheckout selfCheckout)
        // {
        //     selfCheckout.RemoveCustomerFromQueue(customer);
        // }
    }

    model.RemoveCustomer(customer);

    var view = customerViews.Find(v => v.Model == customer);
    if(view != null)
    {
        Destroy(view.gameObject);
        customerViews.Remove(view);
    }

    Debug.Log("Customer removed safely: " + customer.Id);
}


    public void EnableDeleteMode()
    {
        isDeleteMode = true;
        Debug.Log("enable del mode");
    }

    public void DisableDeleteMode()
    {
        isDeleteMode = false;
        Debug.Log("disable del mode");
    }

    public void ToggleDeleteMode()
    {
        isDeleteMode = !isDeleteMode;
        Debug.Log("mode switched");
    }

    public int GetCheckoutMax()
    {
        return CheckoutMax;
    }

    void Start()
{
    StartSimulation();
}

}
