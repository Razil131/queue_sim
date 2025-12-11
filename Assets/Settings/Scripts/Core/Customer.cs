using System.Collections.Generic;
using UnityEngine;

public class Customer
{
    public string Id { get; private set; }
    public int Items { get; private set; }
    public Vector3 Position { get; set; }
    private float itemsProcessedFloat = 0f;
    public int ItemsProcessed => Mathf.FloorToInt(itemsProcessedFloat);
    public float ServiceStartTime { get; set; }
    public float ServiceEndTime { get; set; }
    public ICashRegister CurrentRegister { get; set; }

    public bool AlreadyServed { get; set; }

    public Customer(string id, int items)
    {
        Id = id;
        Items = items;
        itemsProcessedFloat = 0f;
        ServiceStartTime = Time.time;
        ServiceEndTime = 0f;
        CurrentRegister = null;
        AlreadyServed = false;
    }

    public void ChooseRegister(List<ICashRegister> registers)
    {
        //TODO сделать выбор кассы на основе расстояния и количества человек
    }

    public void UpdateProgress(float deltaTime)
    {
        if (CurrentRegister == null) return;

        itemsProcessedFloat += CurrentRegister.ServiceSpeed * deltaTime;
        if (itemsProcessedFloat >= Items)
        {
            AlreadyServed = true;
            ServiceEndTime = Time.time;
            CurrentRegister.ProcessNextCustomer();
            CurrentRegister = null;
        }
    }
    public float GetTotalWaitTime()
    {
        return ServiceEndTime - ServiceStartTime;
    }

    public bool NeedToChooseRegister()
    {
        return CurrentRegister == null && !AlreadyServed;
    }
}