using System.Collections.Generic;
using UnityEngine;

public interface ICashRegister
{
    string Id { get; }
    bool IsSelected { get; set; }
    Vector3 Position { get; set; }
    Vector3 QueueDirection { get; set; }
    RegisterType Type { get; }
    RegisterStatus Status { get; }
    QueueType QueueType { get; set; }
    float ServiceSpeed { get; set; }
    float BreakProbability { get; set; }
    float TimeToRepair {get; set;}
    float NextRepairTime {get;}
    Queue<Customer> Customers { get; }
    Customer NowServing { get; }
    float SPACING_BETWEEN_CUSTOMERS { get; }
    List<Customer> WalkingCustomers { get; }
    void RegisterWalkingCustomer(Customer customer);
    void AddCustomerFromWalking(Customer customer);
    void RemoveWalkingCustomer(Customer customer);
    bool IsAvailable();
    void Close();
    void Open();
    void ProcessNextCustomer();
    void UpdateQueuePositions();
    void BreakDown(float curTime);
    void Repair();
}
