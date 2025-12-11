using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] TripleToggle speedToggle;
    [SerializeField] SimulationModel model;
    [SerializeField] private SimulationController simController;
    [SerializeField] private TMP_InputField intervalInput;
    [SerializeField] private GameObject GeneralSettingsPanel;
    [SerializeField] private GameObject openMenuButton;
    

    void Awake()
    {
        if(speedToggle != null)
        speedToggle.OnToggleSelected.AddListener(OnSpeedChanged);
        if(intervalInput != null)
        {
            intervalInput.onEndEdit.AddListener(OnIntervalChanged);
            intervalInput.text = simController != null ? simController.GetCustomerSpawnInterval().ToString("0") : "999";
        }
        
    }

    private void OnSpeedChanged(int index)
    {
        switch (index)
        {
            case 0:
            model.SetTimeScaleX1();
            break;
            case 1:
            model.SetTimeScaleX2();
            break;
            case 2:
            model.SetTimeScaleX5();
            break;
        }
    }
    

    private void OnIntervalChanged(string inputText)
    {
        if(float.TryParse(inputText, out float value))
        {
            simController.SetCustomerSpawnInterval(value);
        }
    }

    public void OnAddClicked()
    {
        simController.RequestManualCustomerSpawn();
    }

    public void OnDeleteClicked()
    {
        simController.ToggleDeleteMode();
    }

    public void OnClearClicked()
    {
        simController.RemoveAllCustomers();
    }

    public void BreakAtTPUClicked()
    {
        
    }
    public void IncreaseCustomersClicked()
    {
        
    }
    public void DecreaseCustomersClicked()
    {
        
    }

    public void CloseButtonClicked()
    {
        GeneralSettingsPanel.SetActive(false);
        openMenuButton.SetActive(true);

    }

    public void OpenButtonClicked()
    {
        openMenuButton.SetActive(false);
        GeneralSettingsPanel.SetActive(true);
    }
}