using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class SimulationSaveSystem
{
    public static void Save(SimulationController controller, string path)
    {
        var model = controller.Model;

        SimulationSaveData data = new SimulationSaveData
        {
            customerSpawnInterval = controller.GetCustomerSpawnInterval(),
            minItems = controller.GetMinItems(),
            maxItems = controller.GetMaxItems(),
            timeScale = model.TimeScale,
            registers = new List<CashRegisterSaveData>()
        };

        foreach (var reg in model.Registers)
        {
            if (reg is StaffedCashRegister staffed)
            {
                data.registers.Add(new CashRegisterSaveData
                {
                    id = staffed.Id,
                    type = staffed.Type,
                    position = staffed.Position,
                    queueDirection = staffed.QueueDirection,
                    serviceSpeed = staffed.ServiceSpeed,
                    breakProbability = staffed.BreakProbability,
                    timeToRepair = staffed.TimeToRepair,
                    status = staffed.Status
                });
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Simulation saved: " + path);
    }

    public static void Load(SimulationController controller, string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("Save file not found");
            return;
        }

        string json = File.ReadAllText(path);
        SimulationSaveData data = JsonUtility.FromJson<SimulationSaveData>(json);

        controller.PauseSimulation();

        controller.View.ClearRegisters();
        controller.View.ClearCustomers();

        controller.Model.Registers.Clear();
        controller.Model.Customers.Clear();

        controller.SetItemRange(data.minItems, data.maxItems);
        controller.SetCustomerSpawnInterval(data.customerSpawnInterval);
        controller.SetTimeScale(data.timeScale);

        foreach (var regData in data.registers)
        {
            var reg = new StaffedCashRegister(
                regData.id,
                QueueType.Normal,
                regData.serviceSpeed,
                regData.breakProbability,
                regData.timeToRepair
            );

            reg.Position = regData.position;
            reg.QueueDirection = regData.queueDirection;

            controller.Model.AddRegister(reg);
        }

        controller.View.RenderRegisters(controller.Model.Registers);

        controller.StartSimulation();
        Debug.Log("Simulation loaded");
    }

}
