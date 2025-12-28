using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class CashRegisterUIPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text idText;
    [SerializeField] private TripleToggle StatusToggle;
    [SerializeField] private Slider itemsInMin;
    [SerializeField] private Slider breakRate;
    private ICashRegister register;

    public void Initialize(ICashRegister register)
    {
        this.register = register;
        idText.text = register.Id[9..];
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
    }

    public void UpdateUI()
    {
        if (register is StaffedCashRegister staffedRegister)
        {
            Debug.Log($"Current values - ServiceSpeed: {staffedRegister.ServiceSpeed}, BreakProbability: {staffedRegister.BreakProbability}, Status: {staffedRegister.Status}");
            if (itemsInMin != null)
                itemsInMin.value = staffedRegister.ServiceSpeed;

            if (breakRate != null)
                breakRate.value = staffedRegister.BreakProbability;

            if (StatusToggle != null)
            {
                int index = 0;
                if (register.Status == RegisterStatus.Open) index = 1;
                else if (register.Status == RegisterStatus.Broken) index = 2;
                StatusToggle.SetStateByIndex(index);
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
                staffedRegister.BreakDown();
            break;
        }
    }

    private void OnItemsInMinChaged(float value)
    {
        if(register is StaffedCashRegister staffedRegister)
        {
            staffedRegister.ServiceSpeed = value;
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
        
    }

}

//TODO разбить этот файл на несколько чтобы логичнее было