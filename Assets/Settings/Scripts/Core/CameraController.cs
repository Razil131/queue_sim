using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [Header("Settings")]
    public float panSpeed = 1f;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public LayerMask layer;

    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Update()
    {
        HandleCameraPan();
    }

    void HandleCameraPan()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 0f, layer);
            if(hit.collider != null)
            {
                lastMousePosition = Input.mousePosition;
                isDragging = true;   
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            Vector3 move = new Vector3(-delta.x * panSpeed * Time.deltaTime, -delta.y * panSpeed * Time.deltaTime, 0);
            transform.position += move;

            float clampedX = Mathf.Clamp(transform.position.x, minBounds.x, maxBounds.x);
            float clampedY = Mathf.Clamp(transform.position.y, minBounds.y, maxBounds.y);
            transform.position = new Vector3(clampedX, clampedY, transform.position.z);

            lastMousePosition = Input.mousePosition;
        }
    }
}
