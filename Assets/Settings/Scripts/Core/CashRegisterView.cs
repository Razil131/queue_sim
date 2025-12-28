using UnityEngine;

public class CashRegisterView : MonoBehaviour
{
    [SerializeField] private GameObject outline;
    [SerializeField] private GameObject seller;
    [SerializeField] private GameObject progressBar;
    private string registerId;

    public void Initialize(string id)
    {
        registerId = id;
        outline.SetActive(false);
        seller.SetActive(true);
        SetUIPanelActive(false);
    }

    private GameObject GetUIPanel()
    {
        Transform panelTransform = transform.Find("Canvas/CashRegisterUIPanel");
        return panelTransform?.gameObject;
    }

    private void SetUIPanelActive(bool active)
    {
        GameObject panel = GetUIPanel();
        if (panel != null)
        {
            panel.SetActive(active);
        }
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

    void OnMouseDown()
    {
        GameObject panel = GetUIPanel();
        if (panel != null)
        {
            if(panel.activeSelf)
                panel.SetActive(false);
            else
            {
            var panelController = panel.GetComponent<CashRegisterUIPanel>();
            panelController?.UpdateUI();
            panel.SetActive(true);
            }
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

    public void SetProgress(float progress)
    {
        Transform fill = progressBar.transform.Find("fill");
        if(fill != null)
        {
            fill.localScale = new Vector3(progress, 1f, 1f);
        }
    }
}
