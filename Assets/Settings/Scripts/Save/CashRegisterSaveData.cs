using System;
using UnityEngine;

[Serializable]
public class CashRegisterSaveData
{
    public string id;
    public RegisterType type;
    public Vector3 position;
    public Vector3 queueDirection;
    public float serviceSpeed;
    public float breakProbability;
    public float timeToRepair;
    public RegisterStatus status;
}
