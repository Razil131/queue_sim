using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Customer
{
    public string Id { get; private set; }
    public int Items { get; private set; }
    public Vector3 Position { get; set; }
    private float itemsProcessedFloat = 0f;
    private float lastRecheckTime = 0f;
    private const float RECHECK_INTERVAL = 1.5f;
    public int ItemsProcessed => Mathf.FloorToInt(itemsProcessedFloat);
    public float ServiceStartTime { get; set; }
    public float ServiceEndTime { get; set; }
    public ICashRegister CurrentRegister { get; set; }
    public Vector3 TargetPosition { get; private set; }
    public CustomerState State { get; private set; }
    public bool AlreadyServed => State == CustomerState.Served;

    private readonly SimulationModel _model;
    private const float SPAWN_AREA_MIN_X = -5f;
    private const float SPAWN_AREA_MAX_X = -10f;
    private const float SPAWN_AREA_MIN_Y = 0f;
    private const float SPAWN_AREA_MAX_Y = 10f;

    public Customer(string id, int items,  SimulationModel model)
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
        _model = model;
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

        ICashRegister bestRegister = null;
        float bestScore = float.MaxValue;

        foreach (var register in availableRegisters)
        {
            int walkingCount = register.WalkingCustomers.Count;
            
            if (allCustomers != null)
            {
                foreach (var c in allCustomers)
                {
                    if (c != this && 
                        c.State == CustomerState.WalkingToQueue && 
                        c.CurrentRegister == register)
                    {
                        walkingCount++;
                    }
                }
            }

            int queueLength = register.Customers.Count;
            int totalAhead = queueLength + walkingCount;

            float spacing = register.SPACING_BETWEEN_CUSTOMERS;
            Vector3 queueDirection = register.QueueDirection.normalized;
            Vector3 queueEndPosition = register.Position + queueDirection * spacing * (totalAhead + 1);

            float distance = Vector3.Distance(Position, queueEndPosition);
            float score = distance + totalAhead*2;

            if (score < bestScore)
            {
                bestScore = score;
                bestRegister = register;
            }
        }

        if (bestRegister != null)
        {
            CurrentRegister = bestRegister;
            State = CustomerState.WalkingToQueue;
            bestRegister.RegisterWalkingCustomer(this);
            
            Debug.Log($"{Id} is WALKING to {bestRegister.Id} (Estimated position: {bestRegister.Customers.Count + bestRegister.WalkingCustomers.Count + 1})");        
        }
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
            // else if (tempRegister is SelfCheckout selfCheckout)
            // {
            //     selfCheckout.ClearNowServing();
            //     tempRegister.ProcessNextCustomer();
            // }
        }
    }

    private void RecheckBestRegister()
    {
        if (CurrentRegister == null || !CurrentRegister.IsAvailable())
        {
            SwitchToBestAvailableRegister();
            return;
        }

        var allRegisters = _model.Registers;
        var availableRegisters = allRegisters.Where(r => r.IsAvailable()).ToList();

        if (availableRegisters.Count == 0) return;

        float currentScore = CalculateRegisterScore(CurrentRegister);

        ICashRegister bestAlternative = null;
        float bestScore = currentScore * 0.8f;

        foreach (var register in availableRegisters)
        {
            if (register == CurrentRegister) continue;
            
            float score = CalculateRegisterScore(register);
            if (score < bestScore)
            {
                bestScore = score;
                bestAlternative = register;
            }
        }

        if (bestAlternative != null)
        {
            Debug.Log($"{Id} SWITCHING from {CurrentRegister.Id} to {bestAlternative.Id} " +
                    $"(Old score: {currentScore:F1}, New score: {bestScore:F1})");
            SwitchRegister(bestAlternative);
        }
    }

    private float CalculateRegisterScore(ICashRegister register)
    {
        int walkingCount = register.WalkingCustomers.Count(c => 
            c != this && 
            c.State == CustomerState.WalkingToQueue
        );
        
        int queueLength = register.Customers.Count;
        int totalAhead = queueLength + walkingCount;

        float spacing = register.SPACING_BETWEEN_CUSTOMERS;
        Vector3 queueDirection = register.QueueDirection.normalized;
        Vector3 queueEndPosition = register.Position + queueDirection * spacing * (totalAhead + 1);

        float distance = Vector3.Distance(Position, queueEndPosition);
        return distance + totalAhead * 3f;
    }

    private void SwitchRegister(ICashRegister newRegister)
    {
        if (CurrentRegister != null)
        {
            CurrentRegister.RemoveWalkingCustomer(this);
        }

        CurrentRegister = newRegister;
        newRegister.RegisterWalkingCustomer(this);

        Vector3 newPosition = CalculateQueueEndPosition(newRegister);
        UpdateTargetPosition(newPosition);
    }

    private Vector3 CalculateQueueEndPosition(ICashRegister register)
    {
        int walkingCount = register.WalkingCustomers.Count(c => 
            c != this && 
            c.State == CustomerState.WalkingToQueue
        );
        
        int queueLength = register.Customers.Count;
        int totalAhead = queueLength + walkingCount;

        float spacing = register.SPACING_BETWEEN_CUSTOMERS;
        Vector3 queueDirection = register.QueueDirection.normalized;
        return register.Position + queueDirection * spacing * (totalAhead + 1);
    }

    private void SwitchToBestAvailableRegister()
    {
        var availableRegisters = _model.Registers.Where(r => r.IsAvailable()).ToList();
        if (availableRegisters.Count == 0) return;

        ICashRegister bestRegister = null;
        float bestScore = float.MaxValue;

        foreach (var register in availableRegisters)
        {
            float score = CalculateRegisterScore(register);
            if (score < bestScore)
            {
                bestScore = score;
                bestRegister = register;
            }
        }

        if (bestRegister != null)
        {
            SwitchRegister(bestRegister);
        }
    }

    public void UpdateMovement(float deltaTime)
    {
        if (State == CustomerState.WalkingToQueue && 
            Time.time - lastRecheckTime > RECHECK_INTERVAL)
        {
            RecheckBestRegister();
            lastRecheckTime = Time.time;
        }

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
            State = CustomerState.InQueue;
            Debug.Log($"{Id} reached physical queue position at {CurrentRegister.Id}");
            
            CurrentRegister.AddCustomerFromWalking(this);
            
            if (CurrentRegister.NowServing == null && CurrentRegister.IsAvailable())
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
