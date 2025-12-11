using System.Collections.Generic;
using UnityEngine;


public class StaffedCashRegister : ICashRegister
{
    public string Id { get; private set; }
    public bool IsSelected { get; set; }
    public Vector3 Position { get; set; }
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
    }

    public bool IsAvailable()
    {
        return Status == RegisterStatus.Open;
    }

    public void Close()
    {
        Status = RegisterStatus.Closed;
        foreach (var customer in Customers)
        {
            customer.CurrentRegister = null;
        }
    }
    public void Open()
    {
        Status = RegisterStatus.Open;
    }

    public void ProcessNextCustomer()
    {

        if (Customers.Count > 0 && Status == RegisterStatus.Open)
        {
            NowServing = Customers.Dequeue();
        }
        //TODO реализовать начало обслуживания клиента
    }

    public void BreakDown()
    {
        Status = RegisterStatus.Broken;
    }

    public void Repair()
    {
        if (Status == RegisterStatus.Broken)
            Status = RegisterStatus.Open;

    }




}