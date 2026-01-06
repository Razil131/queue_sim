using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[Serializable]
public class SimulationSaveData
{
    public float customerSpawnInterval;
    public int minItems;
    public int maxItems;
    public float timeScale;
    public List<CashRegisterSaveData> registers;
}
