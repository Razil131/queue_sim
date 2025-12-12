using System.Collections.Generic;
using UnityEngine;


public class StaffedCashRegister : ICashRegister
{
    public string Id { get; private set; }
    public bool IsSelected { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 QueueDirection { get; set; }
    public RegisterType Type { get; private set; }
    public RegisterStatus Status { get; private set; }
    public QueueType QueueType { get; private set; }
    public float ServiceSpeed { get; set; }
    public float BreakProbability { get; set; }
    public Queue<Customer> Customers { get; private set; }

    public Customer NowServing { get; private set; }



    public StaffedCashRegister(string id, QueueType queueType, float serviceSpeed, float breakProbability)
    {
        Id = id;
        Type = RegisterType.Staffed;
        Status = RegisterStatus.Open;
        QueueType = queueType;
        ServiceSpeed = serviceSpeed;
        BreakProbability = breakProbability;
        Customers = new Queue<Customer>();
        QueueDirection = new Vector3(-1f, 0f, 0f);
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
        }
    }

    private void UpdateQueuePositions()
    {
        int index = 0;
        float spacingBetweenCustomers = 1f;

        foreach (var customer in Customers)
        {
            Vector3 queuePosition = Position + (QueueDirection.normalized * spacingBetweenCustomers * (index + 1));
            customer.UpdateTargetPosition(queuePosition);
            index++;
        }
    }

    public void BreakDown()
    {
        Status = RegisterStatus.Broken;

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

    public void Repair()
    {
        if (Status == RegisterStatus.Broken)
        {
            Status = RegisterStatus.Open;
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

        return found;
    }
}
