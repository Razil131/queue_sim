using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] TripleToggle speedToggle;
    [SerializeField] SimulationModel model;
    [SerializeField] private SimulationController simController;
    [SerializeField] private TMP_InputField intervalInput;
    [SerializeField] private TMP_InputField minInput;
    [SerializeField] private TMP_InputField maxInput;
    [SerializeField] private GameObject GeneralSettingsPanel;
    [SerializeField] private GameObject openMenuButton;
    [SerializeField] private CashRegisterPlacer cashRegisterPlacer;
    [SerializeField] private TMP_Text registerCountText;
    [SerializeField] private GameObject addRegisterModeOnButton;
    [SerializeField] private GameObject addRegisterModeOffButton;
    [SerializeField] private GameObject exitMenu;

    private bool isMenuActive = true;
    
    void Awake()
    {
        if(speedToggle != null)
            speedToggle.OnToggleSelected.AddListener(OnSpeedChanged);
        if(intervalInput != null)
        {
            intervalInput.onEndEdit.AddListener(OnIntervalChanged);
            intervalInput.text = simController != null ? (60/simController.GetCustomerSpawnInterval()).ToString("0") : "999";
        }

        if(minInput != null)
        {
            minInput.onEndEdit.AddListener(OnMinItemsChanged);
            minInput.text = simController != null ? simController.GetMinItems().ToString("0") : "999";
        }

        if(maxInput != null)
        {
            maxInput.onEndEdit.AddListener(OnMaxItemsChanged);
            maxInput.text = simController != null ? simController.GetMaxItems().ToString("0") : "999";
        }
        if(exitMenu != null)
        {
            exitMenu.SetActive(false);
        }
    }

    void Update()
    {
        if(cashRegisterPlacer != null && cashRegisterPlacer.IsPlacingMode)
        {
            cashRegisterPlacer.UpdatePlacePrev();
        }

        UpdateRegisterCountText();

        if (Input.GetKeyDown(KeyCode.Tab))
            if (isMenuActive == false)
            {
                {
                    OpenButtonClicked();
                }
            }
            else
            {
                CloseButtonClicked();
            }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitMenu.SetActive(true);
        }
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

    private void OnMinItemsChanged(string inputText)
    {
        if(int.TryParse(inputText, out int value))
        {
            simController.SetMinItems(value);
        }
    }

    private void OnMaxItemsChanged(string inputText)
    {
        if(int.TryParse(inputText, out int value))
        {
            simController.SetMaxItems(value);
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
        isMenuActive = false;

    }

    public void OpenButtonClicked()
    {
        openMenuButton.SetActive(false);
        GeneralSettingsPanel.SetActive(true);
        isMenuActive = true;
    }

    public void OnAddRegisterClicked()
    {
    int maxRegisters = simController.GetCheckoutMax();
    cashRegisterPlacer.EnablePlaceMode(maxRegisters);
    if(simController.IsPaused == false)
        {
        simController.PauseSimulation();
        }
    addRegisterModeOnButton.SetActive(false);
    addRegisterModeOffButton.SetActive(true);
    }   

    public void OnCancelPlacementClicked()
    {
        cashRegisterPlacer.DisablePlaceMode();
        if(simController.IsPaused == true)
        {
        simController.PauseSimulation();
        }
        addRegisterModeOffButton.SetActive(false);
        addRegisterModeOnButton.SetActive(true);
    }

    public void ExitMenuYesClicked()
    {
        simController.SaveSimulation();
        SceneManager.LoadScene("MainMenu");
    }

    public void ExitMenuNoClicked()
    {
        exitMenu.SetActive(false);
    }



    }
    //FIXME это исправить
    /*
    ВАЖНО
    После удаления кассы ставишь новую кассу почему то последняя поставленная касса удаляется и появляется ошибка
    1) исправить удаление клиентов на кассах (я не могу там ужас щас вообще не работает удаление отдельных клиентов, потом уже это щас не сильно горит)
    2) когда кассу закрываешь и заново открываешь странное поведение у кастомеров, те кто стоял в очереди замирают, а другие кастомеры через них начинают проходить в очередь
    3) когда касса удаляется, очередь не пропадает и типы в чем то продолжают обслуживаться
    */

    //TODO это сделать
    /*
    1) панельку кассы +
    2) кассы должны сами чиница +
    3) хотя бы заглушки для статистики
    4) кассы можно удалять через панельку +
    5) пауза на пробел +
    6) мастабирование камеры на колесико +
    7) прогрессбар для клиента и количество товаров в идеале +(прогрессбар)
    8) статистека
    9) главное меню
    */