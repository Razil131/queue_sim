using UnityEngine;
using UnityEngine.EventSystems;


public class CashRegisterPlacer : MonoBehaviour
{
    private int maxRegisters;
    private int currentRegisterCount;
    [SerializeField] GameObject ghostRegister;
    
    [SerializeField] SimulationController simController;

    [SerializeField] private GridView snapGrid;
    private GameObject ghostRegisterInstance;
    private bool isPlacingMode = false;

    private Vector2Int[] forbiddenArea;
    Color ValidPlaceColor;
    Color InvalidPlaceColor;
    private int registerWidth;
    private int registerHeight;

    public int CurrentRegisterCount => currentRegisterCount;
    public int MaxRegisters => maxRegisters;
    public bool IsPlacingMode => isPlacingMode;

    void Awake()
    {
        forbiddenArea = new Vector2Int[] {new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(0, 1)};
        registerWidth = 2;
        registerHeight = 1;
        ValidPlaceColor = Color.green;
        InvalidPlaceColor = Color.red;
        isPlacingMode = false;
    }

    public void EnablePlaceMode(int maxRegisters)
    {
        this.maxRegisters = maxRegisters;
        currentRegisterCount = simController.Model.Registers.Count;

        if (ghostRegisterInstance == null)
        {
            ghostRegisterInstance = Instantiate(ghostRegister);
            ghostRegisterInstance.SetActive(false);
        }

        ghostRegisterInstance.SetActive(true);
        isPlacingMode = true;
        UpdatePlacePrev();
    }

    public void DisablePlaceMode()
    {
        isPlacingMode = false;
        if (ghostRegisterInstance != null)
            ghostRegisterInstance.SetActive(false);
    }

    public void UpdatePlacePrev()
    {
        if(!isPlacingMode) return;

        if (ghostRegisterInstance == null)
        {
            Debug.LogError("Ghost register instance is null!");
            return;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        ghostRegisterInstance.transform.position = mouseWorldPos;

        Vector2Int gridPos = snapGrid.WorldToGrid(mouseWorldPos);
        if (!snapGrid.IsValidPos(gridPos))
        {
            SetColor(InvalidPlaceColor);
            return;
        }

        bool isValid = CheckValidPlace(gridPos);

        SetColor(isValid ? ValidPlaceColor : InvalidPlaceColor);

        if (Input.GetMouseButtonDown(0))
        {
            if (IsClickOnBackground())
            {
                if (isValid)
                {
                    PlaceRegister(gridPos);
                }
            }
        }
    }

    public bool CheckValidPlace(Vector2Int gridPos)
    {
        if(currentRegisterCount >= maxRegisters)
        {
            return false;
        }

        for(int x = 0; x < registerWidth; x++)
        {
            for(int y = 0; y < registerHeight; y++)
            {
                Vector2Int pos = new Vector2Int(gridPos.x + x, gridPos.y + y);
                if (!snapGrid.IsValidPos(pos))
                {
                    return false;
                }
            }
        }

        Vector2Int[] occupPos = snapGrid.GetRegisterOccupiedPos(gridPos, registerWidth, registerHeight);
        foreach(var pos in occupPos)
        {
            if (IsCellOcup(pos))
            {
                return false;
            }
        }

        foreach(var pos in occupPos)
        {
            if (IsForbiddenArea(pos))
            {
                return false;
            }
        }
        return true;
    }

    public bool IsCellOcup(Vector2Int pos)
    {
        foreach(var register in simController.Model.Registers)
        {
            Vector2Int registerGridPos = snapGrid.WorldToGrid(register.Position);
            Vector2Int[] registerOccup = snapGrid.GetRegisterOccupiedPos(registerGridPos, registerWidth, registerHeight);
            if(System.Array.Exists(registerOccup, p => p == pos))
                return true;
        }

        foreach (var customer in simController.Model.Customers)
        {
            Vector2Int customerGridPos = snapGrid.WorldToGrid(customer.Position);
            if(customerGridPos == pos)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsForbiddenArea(Vector2Int pos)
    {
        foreach(var offset in forbiddenArea)
        {
            Vector2Int checkPos = pos + offset;
            if (!snapGrid.IsValidPos(checkPos))
            {
                continue;
            }

            if (IsCellOcup(checkPos))
            {
                return true;
            }
        }

        return false;
    }

    public void PlaceRegister(Vector2Int gridPos)
    {
        if (simController.Model.Registers.Count >= simController.GetCheckoutMax())
        {
            Debug.LogWarning("Max registers reached!");
            return;
        }

        if (!CheckValidPlace(gridPos))
        {
            return;
        }

        var newRegister = new StaffedCashRegister($"Register_{currentRegisterCount + 1}", QueueType.Normal, 1f, 0.01f);
        Vector3 worldPos = snapGrid.GetRegisterSpawnPosition(gridPos.x, gridPos.y, registerWidth, registerHeight);
        newRegister.Position = worldPos;

        simController.Model.AddRegister(newRegister);
        currentRegisterCount++;
        simController.View.RenderRegisters(simController.Model.Registers);
    }

    void SetColor(Color color)
    {
        SpriteRenderer sr = ghostRegisterInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = color;
    }


    bool IsClickOnBackground()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

        Vector3 mousePos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        mouseWorldPos.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null)
        {
            return hit.collider.gameObject.layer == LayerMask.NameToLayer("Background");
        }

        return false;
    }
}