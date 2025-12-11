using System.Collections.Generic;
using UnityEngine;

public class SimulationModel : MonoBehaviour
{

    private List<ICashRegister> registers = new List<ICashRegister>();
    private List<Customer> customers = new List<Customer>();

    [SerializeField] private float currentTime = 0f;
    [SerializeField] private float timeScale = 1f;

    private const float TIME_SCALE_X1 = 1f;
    private const float TIME_SCALE_X2 = 2f;
    private const float TIME_SCALE_X5 = 5f;

    public float CurrentTime => currentTime;
    public float TimeScale => timeScale;
    public List<ICashRegister> Registers => registers;
    public List<Customer> Customers => customers;

    void Start()
    {
    }

    void Update()
    {
        float deltaTime = Time.deltaTime * timeScale;
        currentTime += deltaTime;

        foreach (var customer in customers)
        {
            if (customer.NeedToChooseRegister() && !customer.AlreadyServed)
            {
                customer.ChooseRegister(registers);
            }
            else
            {
                customer.UpdateProgress(deltaTime);
            }
        }
    }

    public void SetTimeScale(float scale)
    {
        if (scale > 0f)
        {
            timeScale = scale;
        }
        else
        {
            Debug.LogWarning("Time scale must be greater than 0!");
        }
    }

    public void SetTimeScaleX1() => SetTimeScale(TIME_SCALE_X1);
    public void SetTimeScaleX2() => SetTimeScale(TIME_SCALE_X2);
    public void SetTimeScaleX5() => SetTimeScale(TIME_SCALE_X5);

    public void AddRegister(ICashRegister register)
    {
        if (register != null && !registers.Contains(register))
        {
            registers.Add(register);
        }
    }

    public void RemoveRegister(ICashRegister register)
    {
        if (register != null && registers.Contains(register))
        {
            registers.Remove(register);
        }
    }

    public void SpawnCustomer(string id, int items)
    {
        Customer newCustomer = new Customer(id, items);
        customers.Add(newCustomer);
    }

    public void RemoveCustomer(Customer customer)
    {
        if (customer != null && customers.Contains(customer))
        {
            customers.Remove(customer);
        }
    }
}
