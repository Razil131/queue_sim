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

        var collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }
        collider.isTrigger = true;

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

    void OnTriggerStay2D(Collider2D other)
    {
        if (simController == null || model == null) return;

        if (!simController.IsDeleteMode)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = transform.position.z;

            if (GetComponent<Collider2D>()?.bounds.Contains(mousePos) ?? false)
            {
                simController.RemoveCustomerSafe(model);
            }
        }
    }
}