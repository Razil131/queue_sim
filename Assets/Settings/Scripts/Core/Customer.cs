using System.Collections.Generic;
using System.Linq;
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
    public Vector3 TargetPosition { get; private set; }
    public CustomerState State { get; private set; }

    public bool AlreadyServed => State == CustomerState.Served;

    private const float SPAWN_AREA_MIN_X = 0f;
    private const float SPAWN_AREA_MAX_X = 5f;
    private const float SPAWN_AREA_MIN_Y = 0f;
    private const float SPAWN_AREA_MAX_Y = 5f;

    public Customer(string id, int items)
    {
        Id = id;
        Items = items;
        itemsProcessedFloat = 0f;
        ServiceStartTime = 0f;
        ServiceEndTime = 0f;
        CurrentRegister = null;
        State = CustomerState.Idle;
        Position = GetRandomSpawnPosition();
        TargetPosition = Position;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(SPAWN_AREA_MIN_X, SPAWN_AREA_MAX_X);
        float y = Random.Range(SPAWN_AREA_MIN_Y, SPAWN_AREA_MAX_Y);
        return new Vector3(x, y, 0f);
    }

    public void ChooseRegister(List<ICashRegister> registers, List<Customer> allCustomers)
    {
        if (registers == null || registers.Count == 0)
        {
            Debug.LogWarning($"{Id}: No registers available");
            return;
        }

        var availableRegisters = registers.Where(r => r.IsAvailable()).ToList();

        if (availableRegisters.Count == 0)
        {
            Debug.LogWarning($"{Id}: All registers are unavailable");
            return;
        }

        ICashRegister bestRegister = SelectBestRegister(availableRegisters);

        if (bestRegister != null)
        {
            CurrentRegister = bestRegister;
            TargetPosition = CalculateQueuePosition(bestRegister, allCustomers);
            State = CustomerState.WalkingToQueue;

            Debug.Log($"{Id} chose {bestRegister.Id} (Queue: {bestRegister.Customers.Count}, Distance: {Vector3.Distance(Position, bestRegister.Position):F1})");
        }
    }

    private ICashRegister SelectBestRegister(List<ICashRegister> availableRegisters)
    {
        ICashRegister bestRegister = null;
        float bestScore = float.MaxValue;

        foreach (var register in availableRegisters)
        {
            float distance = Vector3.Distance(Position, register.Position);
            int queueLength = register.Customers.Count;

            float score = distance + (queueLength * 300f);

            if (score < bestScore)
            {
                bestScore = score;
                bestRegister = register;
            }
        }

        return bestRegister;
    }

    private Vector3 CalculateQueuePosition(ICashRegister register, List<Customer> allCustomers)
    {
        int walkingToQueueCount = 0;
        if (allCustomers != null)
        {
            foreach (var c in allCustomers)
            {
                if (c.State == CustomerState.WalkingToQueue && c.CurrentRegister == register && c != this)
                {
                    walkingToQueueCount++;
                }
            }
        }

        int positionInQueue = register.Customers.Count + walkingToQueueCount;
        float spacingBetweenCustomers = 1f;
        Vector3 offset = register.QueueDirection.normalized * spacingBetweenCustomers * positionInQueue;
        return register.Position + offset;
    }

    public void UpdateProgress(float deltaTime)
    {
        if (State != CustomerState.BeingServed) return;
        if (CurrentRegister == null) return;
        if (CurrentRegister.NowServing != this) return;

        itemsProcessedFloat += CurrentRegister.ServiceSpeed * deltaTime;
        if (itemsProcessedFloat >= Items)
        {
            State = CustomerState.Served;
            ServiceEndTime = Time.time;
            ICashRegister tempRegister = CurrentRegister;
            CurrentRegister = null;

            if (tempRegister is StaffedCashRegister staffed)
            {
                staffed.ClearNowServing();
                tempRegister.ProcessNextCustomer();
            }
            else if (tempRegister is SelfCheckout selfCheckout)
            {
                selfCheckout.ClearNowServing();
                tempRegister.ProcessNextCustomer();
            }
        }
    }

    public void UpdateMovement(float deltaTime)
    {
        float distanceToTarget = Vector3.Distance(Position, TargetPosition);

        if (distanceToTarget > 0.1f)
        {
            float moveSpeed = 2f;
            Vector3 direction = (TargetPosition - Position).normalized;
            Position += direction * moveSpeed * deltaTime;
        }
        else
        {
            Position = TargetPosition;
            OnReachedTargetPosition();
        }
    }

    private void OnReachedTargetPosition()
    {
        if (State == CustomerState.WalkingToQueue && CurrentRegister != null)
        {
            CurrentRegister.Customers.Enqueue(this);
            State = CustomerState.InQueue;
            Debug.Log($"{Id} reached queue position and joined {CurrentRegister.Id}'s queue");

            if (CurrentRegister.NowServing == null)
            {
                CurrentRegister.ProcessNextCustomer();
            }
        }
        else if (State == CustomerState.WalkingToRegister)
        {
            State = CustomerState.BeingServed;
            Debug.Log($"{Id} reached register and started being served");
        }
    }

    public float GetTotalWaitTime()
    {
        return ServiceEndTime - ServiceStartTime;
    }

    public bool NeedToChooseRegister()
    {
        return State == CustomerState.Idle;
    }

    public bool IsMoving()
    {
        return Vector3.Distance(Position, TargetPosition) > 1f;
    }

    public void UpdateTargetPosition(Vector3 newTarget)
    {
        TargetPosition = newTarget;
    }

    public void StartWalkingToRegister()
    {
        if (State == CustomerState.InQueue)
        {
            State = CustomerState.WalkingToRegister;
            Debug.Log($"{Id} started walking to register");
        }
    }
}
