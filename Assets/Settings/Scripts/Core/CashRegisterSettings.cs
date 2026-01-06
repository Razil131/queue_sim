using UnityEngine;

public class CashRegisterSettings
{
    public Vector3 QueueDirection { get; set; }
    public RegisterStatus Status { get; set; }
    public QueueType QueueType { get; set; }
    public float ServiceSpeed { get; set; }
    public float BreakProbability { get; set; }
    public float TimeToRepair {get; set;}

    public CashRegisterSettings(RegisterStatus status, Vector3 queueDirection, QueueType queueType,  float serviceSpeed, float breakProbability, float timeToRepair)
    {
        Status = status;
        QueueType = queueType;
        ServiceSpeed = serviceSpeed;
        BreakProbability = breakProbability;
        TimeToRepair = timeToRepair;
        QueueDirection = queueDirection;
    }
}