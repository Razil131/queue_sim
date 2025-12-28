using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TripleToggle : MonoBehaviour
{
    [SerializeField] private Toggle[] toggles;
    [SerializeField] private GameObject[] outlines;
    public UnityEvent<int> OnToggleSelected;

    void Start()
    {
        for (int i = 0; i < toggles.Length; i++)
    {
        toggles[i].isOn = (i == 0);
        outlines[i].SetActive(i == 0);
    }
        for(int i = 0; i < toggles.Length; i++)
        {
            int index = i;
            toggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(index, isOn));
            outlines[i].SetActive(toggles[i].isOn);
        }
    }

    private void OnToggleChanged(int index, bool isOn)
    {
        if(!isOn) return;
        for (int i = 0; i < outlines.Length; i++)
    {
        outlines[i].SetActive(i == index);
        if (i != index) toggles[i].isOn = false;
    }

    OnToggleSelected?.Invoke(index);
    }
    
    public void SetStateByIndex(int index)
    {
        if (index >= 0 && index < toggles.Length)
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                toggles[i].SetIsOnWithoutNotify(false);
                outlines[i].SetActive(false);
            }

            toggles[index].SetIsOnWithoutNotify(true);
            outlines[index].SetActive(true);
        }
    }

}