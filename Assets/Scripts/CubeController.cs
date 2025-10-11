using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class CubeController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 targetPosition;
    private bool isMoving = false;

    private float lastLogTime = 0f;

    void Start()
    {
        targetPosition = transform.position;
        Debug.Log("[CubeController] Initialized - Click anywhere to move!");
        
        #if ENABLE_INPUT_SYSTEM
        Debug.Log("[CubeController] Using NEW Input System");
        if (Mouse.current == null)
        {
            Debug.LogError("[CubeController] Mouse.current is NULL! Input System not working!");
        }
        else
        {
            Debug.Log("[CubeController] Mouse.current is available ✓");
        }
        #else
        Debug.Log("[CubeController] Using OLD Input System");
        #endif
    }

    void Update()
    {
        // Log every 5 seconds to confirm Update is running
        if (Time.time - lastLogTime > 5f)
        {
            lastLogTime = Time.time;
            
            #if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                bool leftPressed = Mouse.current.leftButton.isPressed;
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Debug.Log($"[CubeController] Update running - Mouse at {mousePos}, Left button pressed: {leftPressed}");
            }
            #else
            Debug.Log($"[CubeController] Update running - Mouse at {Input.mousePosition}, Left button: {Input.GetMouseButton(0)}");
            #endif
        }

        // Handle input based on which input system is active
        bool clickDetected = false;
        Vector3 inputPosition = Vector3.zero;
        
        #if ENABLE_INPUT_SYSTEM
        // New Input System - Check for mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            clickDetected = true;
            inputPosition = Mouse.current.position.ReadValue();
            Debug.Log("[CubeController] ✓ NEW Input System detected MOUSE click!");
        }
        // New Input System - Check for touch
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            clickDetected = true;
            inputPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            Debug.Log("[CubeController] ✓ NEW Input System detected TOUCH!");
        }
        #else
        // Old Input System - Check for mouse
        if (Input.GetMouseButtonDown(0))
        {
            clickDetected = true;
            inputPosition = Input.mousePosition;
            Debug.Log("[CubeController] ✓ OLD Input System detected MOUSE click!");
        }
        // Old Input System - Check for touch
        else if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                clickDetected = true;
                inputPosition = touch.position;
                Debug.Log("[CubeController] ✓ OLD Input System detected TOUCH!");
            }
        }
        #endif

        if (clickDetected)
        {
            Debug.Log($"[CubeController] Input detected at position: {inputPosition}");
            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"[CubeController] Raycast hit: {hit.collider.gameObject.name} at {hit.point}");
                targetPosition = hit.point;
                targetPosition.y = transform.position.y; // Keep same height
                isMoving = true;
                Debug.Log($"[CubeController] Moving to: {targetPosition}");
            }
            else
            {
                Debug.LogWarning("[CubeController] Raycast missed - no collider hit!");
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
