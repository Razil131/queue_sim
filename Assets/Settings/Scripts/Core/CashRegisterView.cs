using UnityEngine;

public class CashRegisterView : MonoBehaviour
{
    [SerializeField] private GameObject outline;
    private string registerId;

    public void Initialize(string id)
    {
        registerId = id;
        outline.SetActive(false);
    }

    void OnMouseEnter()
    {
        outline.SetActive(true);
    }

    void OnMouseExit()
    {
        outline.SetActive(false);
    }
}
