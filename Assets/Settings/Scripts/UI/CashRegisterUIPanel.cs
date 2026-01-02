using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Xml.Schema;

public class CashRegisterUIPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text idText;
    [SerializeField] private TripleToggle StatusToggle;
    [SerializeField] private Slider itemsInMin;
    [SerializeField] private Slider breakRate;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text itemspermin;
    [SerializeField] private TMP_Text breakRateText;
    [SerializeField] private TMP_InputField breakTime;
    [SerializeField] private Button deleteButton;
    private SimulationController controller;
    private ICashRegister register;

    public void Initialize(ICashRegister register, SimulationController controller)
    {
        this.controller = controller;
        this.register = register;
        idText.text = register.Id[9..];

        itemsInMin.minValue = 0;
        itemsInMin.maxValue = 150;
        itemsInMin.wholeNumbers = true;

        Debug.Log($"Initialize called with register: {register?.Id}");

        UpdateUI();
    }
    void Awake()
    {
        Debug.Log("CashRegisterUIPanel.Awake called");
        if(StatusToggle != null)
            StatusToggle.OnToggleSelected.AddListener(OnStatusChanged);
        if(itemsInMin != null)
            itemsInMin.onValueChanged.AddListener(OnItemsInMinChaged);
        if(breakRate != null)
            breakRate.onValueChanged.AddListener(OnBrakeRateChanged);
        if(closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        if(breakTime != null)
            breakTime.onEndEdit.AddListener(OnBreakTimeChanged);
        if(deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (register is StaffedCashRegister staffedRegister)
        {
            //Debug.Log($"Current values - ServiceSpeed: {staffedRegister.ServiceSpeed*60} per minute, BreakProbability: {staffedRegister.BreakProbability}, Status: {staffedRegister.Status}");
            if (itemsInMin != null)
                itemsInMin.value = staffedRegister.ServiceSpeed*60;

            if (breakRate != null)
                breakRate.value = staffedRegister.BreakProbability;

            if (StatusToggle != null)
            {
                int index = 0;
                if (register.Status == RegisterStatus.Open) index = 1;
                else if (register.Status == RegisterStatus.Broken) index = 2;
                StatusToggle.SetStateByIndex(index);
            }
            if(itemspermin != null)
            {
                itemspermin.text = GetItemsInMin().ToString();
            }
            if(breakRateText != null)
            {
                breakRateText.text = GetBreakRate().ToString("P1");
            }
            if(breakTime != null && !breakTime.isFocused)
            {
                breakTime.text = GetBreakTime().ToString("0");
                
            }
    }
    }

    void OnEnable()
    {
        Debug.Log("CashRegisterUIPanel.OnEnable called");
        UpdateUI();
    }

    private void OnStatusChanged(int index)
    {

        if (register == null)
    {
        Debug.LogError("Register is null in CashRegisterUIPanel");
        return;
    }  
        switch (index)
        {
            case 0:
            register.Close();
            break;
            case 1:
            register.Open();
            break;
            case 2:
            if (register is StaffedCashRegister staffedRegister)
                staffedRegister.BreakDown(controller.GetCurTime());
            break;
        }
    }

    private void OnItemsInMinChaged(float value)
    {
        if(register is StaffedCashRegister staffedRegister)
        {
            staffedRegister.ServiceSpeed = value/60.0f;
        }
    }

    private void OnBrakeRateChanged(float value)
    {
        if(register is StaffedCashRegister staffedRegister)
        {
            staffedRegister.BreakProbability = value;
        }
    }

    private void OnCloseButtonClicked()
    {
        this.gameObject.SetActive(false);
    }

    private float GetItemsInMin()
    {
        if(register is StaffedCashRegister staffed)
            return staffed.ServiceSpeed * 60;
        return 0f;
    }

    private float GetBreakRate()
    {
        if(register is StaffedCashRegister staffed)
            return staffed.BreakProbability;
    return 0f;
    }

    private float GetBreakTime()
    {
        if(register is StaffedCashRegister staffed)
        {
            return staffed.TimeToRepair;
        }
        return 0f;
    }

    private void OnBreakTimeChanged(string inputText)
    {
        if(int.TryParse(inputText, out int value))
        {
            if(register is StaffedCashRegister staffed)
            {
                staffed.TimeToRepair = value;
            }
        }
    }
    
    private void OnDeleteButtonClicked()
    {
        register.Close();
        if(controller != null && register != null)
        {
            controller.RemoveRegister(register);
        }
        this.gameObject.SetActive(false);
    }

}

//TODO разбить этот файл на несколько чтобы логичнее было