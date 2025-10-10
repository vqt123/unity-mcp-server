using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CubeController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        targetPosition = transform.position;
        Debug.Log("[CubeController] Initialized - Click anywhere to move!");
    }

    void Update()
    {
        // Handle input based on which input system is active
        bool clickDetected = false;
        
        #if ENABLE_INPUT_SYSTEM
        // New Input System
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            clickDetected = true;
        }
        #else
        // Old Input System
        if (Input.GetMouseButtonDown(0))
        {
            clickDetected = true;
        }
        #endif

        if (clickDetected)
        {
            Ray ray = Camera.main.ScreenPointToRay(GetMousePosition());
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                targetPosition = hit.point;
                targetPosition.y = transform.position.y; // Keep same height
                isMoving = true;
                Debug.Log($"[CubeController] Moving to: {targetPosition}");
            }
        }

        // Move towards target
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                targetPosition, 
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                Debug.Log("[CubeController] Reached target!");
            }
        }
    }

    private Vector3 GetMousePosition()
    {
        #if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }
        return Vector3.zero;
        #else
        return Input.mousePosition;
        #endif
    }
}
