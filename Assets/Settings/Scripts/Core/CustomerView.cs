using UnityEngine;

public class CustomerView : MonoBehaviour
{
    private Customer model;
    private SimulationController simController;

    public Customer Model => model;

    public void Initialize(Customer customer, SimulationController controller)
    {
        model = customer;
        simController = controller;
        transform.position = model.Position;

        simController?.RegisterCustomerView(this);
    }


    void Update()
    {
        UpdateView();
    }

    public void UpdateView()
    {
        if (model == null) return;
        transform.position = model.Position;
    }

    void OnMouseDown()
    {
        if (simController == null || model == null) return;

        if (!simController.IsDeleteMode)
            return;

        simController.RemoveCustomerSafe(model);
    }
}
