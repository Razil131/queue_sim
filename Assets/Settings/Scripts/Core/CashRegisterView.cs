using UnityEngine;

public class CashRegisterView : MonoBehaviour
{
    [SerializeField] private GameObject outline;
    [SerializeField] private GameObject seller;
    private string registerId;

    public void Initialize(string id)
    {
        registerId = id;
        outline.SetActive(false);
        seller.SetActive(true);
    }

        void Update()
    {
        if (IsMouseOver())
        {
            outline.SetActive(true);
        }
        else
        {
            outline.SetActive(false);
        }
    }

     public void SetBroken()
    {
        seller.SetActive(false);
    }

    public void SetFixed()
    {
        seller.SetActive(true);
    }

    bool IsMouseOver()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = transform.position.z;
        return GetComponent<SpriteRenderer>()?.bounds.Contains(mousePos) ?? false;
    }
}
