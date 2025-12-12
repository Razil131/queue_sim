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
    [SerializeField] private CashRegisterPlacer cashRegisterPlacer;
    [SerializeField] private TMP_Text registerCountText;
    [SerializeField] private GameObject addRegisterModeOnButton;
    [SerializeField] private GameObject addRegisterModeOffButton;
    
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

    void Update()
    {
        if(cashRegisterPlacer != null && cashRegisterPlacer.IsPlacingMode)
        {
            cashRegisterPlacer.UpdatePlacePrev();
        }

        UpdateRegisterCountText();
    }

    void Start()
    {
        
    }

    public void UpdateRegisterCountText()
    {
        if(registerCountText != null && simController != null)
        {
            int current = simController.Model.Registers.Count;
            int max = simController.GetCheckoutMax();
            registerCountText.text = $"{current}/{max}";
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

    public void OnAddRegisterClicked()
    {
    int maxRegisters = simController.GetCheckoutMax();
    cashRegisterPlacer.EnablePlaceMode(maxRegisters);
    simController.PauseSimulation();
    addRegisterModeOnButton.SetActive(false);
    addRegisterModeOffButton.SetActive(true);
    }   

    public void OnCancelPlacementClicked()
    {
        cashRegisterPlacer.DisablePlaceMode();
        simController.PauseSimulation();
        addRegisterModeOffButton.SetActive(false);
        addRegisterModeOnButton.SetActive(true);
    }

    
    }
    //TODO это сделать
    /*
    Исправить режим выставления касс:
    1)На курсоре не появляется касса FIXED
    2)Касса ставится за худом, хотя не должна FIXED
    3)Не учитываются клетки сверху и снизу по игреку FIXED
    4)Кассы ставятся в кастомеров (хз это надо преверить потом после мерджа)
    5)При зажатии кнопки тоже кассы ставятся хотя не должны FIXED(изменена кнопка перемещения)
    6)Кассы по иксу ставятся друг на друга почему FIXED
    7)При старте программы пишется не текущее значение касс а 0/0 FIXED
    */