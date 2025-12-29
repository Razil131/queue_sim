using System.Collections.Generic;
using UnityEngine;


public class StaffedCashRegister : ICashRegister
{
    public string Id { get; private set; }
    public bool IsSelected { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 QueueLastPosition { get; set; }
    public Vector3 QueueDirection { get; set; }
    public RegisterType Type { get; private set; }
    public RegisterStatus Status { get; private set; }
    public QueueType QueueType { get; private set; }
    public float ServiceSpeed { get; set; }
    public float BreakProbability { get; set; }
    public float TimeToRepair {get; set;}
    public float NextRepairTime {get; private set;}
    public Queue<Customer> Customers { get; private set; }
     public List<Customer> WalkingCustomers { get; private set; } = new List<Customer>();
    public Customer NowServing { get; private set; }
    public float SPACING_BETWEEN_CUSTOMERS { get; }
    private CashRegisterView view;



    public StaffedCashRegister(string id, QueueType queueType, float serviceSpeed, float breakProbability, float timeToRepair)
    {
        SPACING_BETWEEN_CUSTOMERS = 1f;
        Id = id;
        Type = RegisterType.Staffed;
        Status = RegisterStatus.Open;
        QueueType = queueType;
        ServiceSpeed = serviceSpeed;
        BreakProbability = breakProbability;
        TimeToRepair = timeToRepair;
        Customers = new Queue<Customer>();
        QueueDirection = new Vector3(-1f, 0f, 0f);
    }

    public void SetView(CashRegisterView view)
    {
        this.view = view;
    }
    public bool IsAvailable()
    {
        return Status == RegisterStatus.Open;
    }

    public void Close()
    {
        Status = RegisterStatus.Closed;

        if (NowServing != null)
        {
            NowServing.CurrentRegister = null;
            NowServing = null;
        }

        foreach (var customer in Customers)
        {
            customer.CurrentRegister = null;
        }
        Customers.Clear();
    }
    public void Open()
    {
        view?.SetFixed();
        Status = RegisterStatus.Open;
    }

    public void ProcessNextCustomer()
    {
        if (NowServing != null) return;

        if (Customers.Count > 0 && Status == RegisterStatus.Open)
        {
            NowServing = Customers.Dequeue();
            NowServing.ServiceStartTime = UnityEngine.Time.time;
            NowServing.StartWalkingToRegister();
            NowServing.UpdateTargetPosition(Position);

            UpdateQueuePositions();

            view?.SetProgress(GetProgress());
        }
    }


    public void UpdateQueuePositions()
    {
        int positionIndex = 1;
        foreach (var customer in Customers)
        {
            if (customer.State == CustomerState.BeingServed || customer.State == CustomerState.Served) continue;
            Vector3 queuePosition = Position + (QueueDirection.normalized * SPACING_BETWEEN_CUSTOMERS * positionIndex);
            customer.UpdateTargetPosition(queuePosition);
            positionIndex++;
        }

        foreach (var customer in WalkingCustomers)
        {
            Vector3 queuePosition = Position + (QueueDirection.normalized * SPACING_BETWEEN_CUSTOMERS * positionIndex);
            customer.UpdateTargetPosition(queuePosition);
            positionIndex++;
        }
    }

    public void RegisterWalkingCustomer(Customer customer)
    {
        WalkingCustomers.Add(customer);
        UpdateQueuePositions();
    }
    
    public void AddCustomerFromWalking(Customer customer)
    {
        if (WalkingCustomers.Remove(customer))
        {
            Customers.Enqueue(customer);
            UpdateQueuePositions();
            Debug.Log($"{customer.Id} physically joined queue at {Id}");
        }
    }
    public void RemoveWalkingCustomer(Customer customer)
    {
        if (WalkingCustomers.Remove(customer))
        {
            UpdateQueuePositions();
            Debug.Log($"{customer.Id} removed from walking list of {Id}");
        }
}

    public void BreakDown(float curTime)
    {
        Status = RegisterStatus.Broken;
        NextRepairTime = curTime+TimeToRepair;
        view?.SetBroken();
        if (NowServing != null)
        {
            NowServing.CurrentRegister = null;
            NowServing.State = CustomerState.Idle;
            NowServing = null;
        }

        foreach (var customer in Customers)
        {
            customer.CurrentRegister = null;
            customer.State = CustomerState.Idle;
        }
        Customers.Clear();
    }

    public void Repair()
    {
        if (Status == RegisterStatus.Broken)
        {
            Status = RegisterStatus.Open;
            view?.SetFixed();
            if (Customers.Count > 0 && NowServing == null)
            {
                ProcessNextCustomer();
            }
        }
    }

    public void ClearNowServing()
    {
        NowServing = null;
    }

    public bool RemoveCustomerFromQueue(Customer customer)
    {
        if (NowServing == customer)
        {
            NowServing = null;
            if (Customers.Count > 0)
            {
                ProcessNextCustomer();
            }
            return true;
        }

        var tempQueue = new Queue<Customer>();
        bool found = false;

        while (Customers.Count > 0)
        {
            var c = Customers.Dequeue();
            if (c == customer)
            {
                found = true;
            }
            else
            {
                tempQueue.Enqueue(c);
            }
        }

        Customers = tempQueue;

        if (found)
        {
            UpdateQueuePositions();
        }

        if (found) UpdateQueuePositions();
        return found;
    }

    public float GetProgress()
    {
        if(NowServing == null || NowServing.Items == 0 ) return 0f;
        float items = NowServing.Items;
        float itemsProcessed = NowServing.ItemsProcessed;
        float newWidth = itemsProcessed/items;
        return Mathf.Clamp01(newWidth);
    }

    public void SetProgress()
    {
        view.SetProgress(GetProgress());
    }
}
