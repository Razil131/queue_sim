using UnityEngine;

public class GridView : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 30;
    public int height = 30;
    public float cellSize = 100f;

    [Header("Prefab")]
    [SerializeField] private GameObject cellPrefab;

    private Vector3 originPosition;

    void Awake()
    {
        originPosition = Vector3.zero;
    }

    public void CreateGrid()
    {
        if (cellPrefab == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = GetCellWorldPosition(x, y);
                GameObject cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";

                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingOrder = 0;
                }
            }

            
        }
    }

    public Vector3 GetCellWorldPosition(int x, int y)
    {
        return new Vector3(x * cellSize, y * cellSize, 0f);
    }

    public Vector3 GetRegisterSpawnPosition(int startX, int startY, float registerWidth, float registerHeight)
    {
    float centerX = (startX + 0.5f) * cellSize;
    float centerY = (startY) * cellSize;

    return new Vector3(centerX, centerY, 0f);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int y = Mathf.FloorToInt(worldPos.y / cellSize);
        return new Vector2Int(x, y);
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {   
        return GetCellWorldPosition(gridPos.x, gridPos.y);
    }

    public bool IsValidPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public Vector2Int[] GetRegisterOccupiedPos(Vector2Int basePos, int width, int height)
    {
        System.Collections.Generic.List<Vector2Int> positions = new System.Collections.Generic.List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                positions.Add(new Vector2Int(basePos.x + x, basePos.y + y));
            }
        }
        return positions.ToArray();
    }
}