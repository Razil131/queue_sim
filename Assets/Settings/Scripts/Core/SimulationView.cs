using UnityEngine;
using System.Collections.Generic;

public class SimulationView : MonoBehaviour
{
    public GridView GridView => gridView;
    private Dictionary<string, GameObject> registerObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> customerObjects = new Dictionary<string, GameObject>();

    [SerializeField] private SimulationController simulationController;

    [Header("Prefabs")]
    [SerializeField] private GameObject cashRegisterPrefab;
    [SerializeField] private GameObject customerPrefab;

    [Header("Containers")]
    [SerializeField] private Transform registersContainer;
    [SerializeField] private Transform customersContainer;

    [SerializeField] private GridView gridView;
    
    public void RenderGrid()
    {
    if (gridView != null && gridView.transform.childCount == 0)
        gridView.CreateGrid();
    }


        public void RenderRegisters(List<ICashRegister> registers)
    {
        foreach (var register in registers)
        {
            if (registerObjects.ContainsKey(register.Id))
            {
                GameObject obj = registerObjects[register.Id];
                obj.transform.position = register.Position;
            }
            else
            {
                GameObject newObj = Instantiate(cashRegisterPrefab, registersContainer);
                CashRegisterView viewComp = newObj.GetComponent<CashRegisterView>();

                if (viewComp != null)
                {
                    viewComp.Initialize(register.Id);
                }

                newObj.transform.position = register.Position;
                registerObjects[register.Id] = newObj;
            }
        }

        List<string> keysToRemove = new List<string>();
        foreach (var key in registerObjects.Keys)
        {
            if (!registers.Exists(r => r.Id == key))
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            Destroy(registerObjects[key]);
            registerObjects.Remove(key);
        }
    }





    public void RenderCustomers(List<Customer> customers){
    foreach (var customer in customers)
        {
            if (customerObjects.ContainsKey(customer.Id))
            {
                var obj = customerObjects[customer.Id];
                var viewComp = obj.GetComponent<CustomerView>();
                viewComp?.UpdateView();
            }
            else
            {
                var newObj = Instantiate(customerPrefab, customersContainer);
                var viewComp = newObj.GetComponent<CustomerView>();
                if (viewComp != null)
                    viewComp.Initialize(customer, simulationController);

                customerObjects[customer.Id] = newObj;
            }
        }

        var keysToRemove = new List<string>();
        foreach (var key in customerObjects.Keys)
        {
            bool exists = customers.Exists(c => c.Id == key);
            if (!exists)
                keysToRemove.Add(key);
        }

        foreach (var key in keysToRemove)
        {
            Destroy(customerObjects[key]);
            customerObjects.Remove(key);
        }
    }

}
