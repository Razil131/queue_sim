using System.Collections.Generic;
using UnityEngine;

public interface ICashRegister
{
    string Id { get; }
    bool IsSelected { get; set; }
    Vector3 Position { get; set; }
    RegisterType Type { get; }
    RegisterStatus Status { get; }
    QueueType QueueType { get; }
    float ServiceSpeed { get; set; }
    float BreakProbability { get; set; }
    Queue<Customer> Customers { get; }
    Customer NowServing { get; }
    bool IsAvailable();
    void Close();
    void Open();
    void ProcessNextCustomer();
    void BreakDown();
    void Repair();
}
