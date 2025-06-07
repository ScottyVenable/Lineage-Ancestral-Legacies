using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CINEMACHINE_PRESENT
using Unity.Cinemachine;
#endif
using UnityEngine.InputSystem;
using Lineage.Debug;

namespace Lineage.Managers
{
    /// <summary>
    /// Manages camera movement, zoom, and panning for the game.
    /// Supports both keyboard (WASD) and mouse (middle-click drag) panning.
    /// Uses Unity Input System for input handling with fallbacks to legacy input.
    /// </summary>
    public class CameraManager : MonoBehaviour
{
    // Singleton instance
    public static CameraManager Instance { get; private set; }    [Header("Camera Settings")]
    public float panSpeed = 20f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float rotationSpeed = 100f;
    public bool smoothMovement = true;
    public float smoothTime = 0.5f;

    [Header("Mouse Panning")]
    public bool invertMousePanX = false;
    public bool invertMousePanY = true;
    [Range(0.1f, 5f)]
    public float mousePanSensitivity = 1f;
    [Range(0.1f, 10f)]
    public float mousePanSpeed = 1f;
    [Tooltip("Stop camera movement immediately when mouse is released")]
    public bool immediateStopOnMouseRelease = true;
    [Tooltip("How responsive the mouse panning feels (higher = more responsive)")]
    [Range(1f, 20f)]
    public float mousePanResponsiveness = 5f;

    [Header("Boundaries")]
    public Vector2 minBounds = new Vector2(-50f, -50f);
    public Vector2 maxBounds = new Vector2(50f, 50f);

    [Header("References")]
    #if CINEMACHINE_PRESENT
    public CinemachineCamera virtualCamera;
    #else
    public Camera virtualCamera;
    #endif
    public InputActionAsset inputActions;    // Public properties
    public bool IsMousePanning => isMiddleMousePressed;
    public Vector3 CurrentTargetPosition => targetPosition;
    private InputAction moveAction;
    private InputAction zoomAction;
    private InputAction middleClickAction;
    private InputAction mousePositionAction;
    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 velocity = Vector3.zero;
    
    // Mouse panning variables
    private bool isMiddleMousePressed = false;
    private Vector3 lastMousePosition;
    private Vector3 mousePanStartPosition;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }        // Initialize if references are missing
        #if CINEMACHINE_PRESENT
        if (virtualCamera == null)
        {
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            Log.Warning($"VirtualCamera reference not set - found CinemachineCamera in scene: {(virtualCamera != null ? virtualCamera.name : "None")}", Log.LogCategory.Systems);
        }
        else
        {
            Log.Info($"CinemachineCamera already assigned: {virtualCamera.name}", Log.LogCategory.Systems);
        }
        #else
        if (virtualCamera == null)
        {
            virtualCamera = Camera.main;
            Log.Warning($"Cinemachine not found. Using Camera.main as fallback: {(virtualCamera != null ? virtualCamera.name : "None")}", Log.LogCategory.Systems);
        }
        else
        {
            Log.Info($"Standard Camera already assigned: {virtualCamera.name}", Log.LogCategory.Systems);
        }
        #endif

        // Test camera access immediately
        Camera testCam = GetActiveCamera();
        Log.Info($"Active camera found during initialization: {(testCam != null ? testCam.name : "NONE")}", Log.LogCategory.Systems);

        // Load the input actions if not set
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            Log.Warning("InputActions reference not set - attempting to load from Resources", Log.LogCategory.Systems);
        }        // Initialize positions
        if (virtualCamera != null)
        {
            // Use the virtual camera's position as the starting target position
            Transform cameraTransform = GetTargetTransform();
            if (cameraTransform != null)
            {
                targetPosition = cameraTransform.position;
                Log.Info($"Camera: Initialized target position to {targetPosition} from {cameraTransform.name}", Log.LogCategory.Systems);
            }
            else
            {
                targetPosition = transform.position;
                Log.Warning("Camera: Using CameraManager transform position as fallback", Log.LogCategory.Systems);
            }
            
            #if CINEMACHINE_PRESENT
            targetZoom = virtualCamera.Lens.OrthographicSize;
            #else
            if (virtualCamera.orthographic)
                targetZoom = virtualCamera.orthographicSize;
            else
                targetZoom = minZoom;
            #endif
        }
    }    private void OnEnable()
    {
        Log.Info("Camera: OnEnable called", Log.LogCategory.Systems);
        
        if (inputActions != null)
        {
            Log.Info($"Camera: InputActions asset found: {inputActions.name}", Log.LogCategory.Systems);
            
            // Enable the action maps first
            inputActions.Enable();
            Log.Info("Camera: InputActions enabled", Log.LogCategory.Systems);
            
            // Find and configure input actions
            moveAction = inputActions.FindAction("Player/Move");
            zoomAction = inputActions.FindAction("UI/ScrollWheel");
            middleClickAction = inputActions.FindAction("UI/MiddleClick");
            mousePositionAction = inputActions.FindAction("UI/Point");

            Log.Info($"Camera: Action mapping results - Move: {(moveAction != null ? "Found" : "NOT FOUND")}, " +
                     $"Zoom: {(zoomAction != null ? "Found" : "NOT FOUND")}, " +
                     $"MiddleClick: {(middleClickAction != null ? "Found" : "NOT FOUND")}, " +
                     $"MousePosition: {(mousePositionAction != null ? "Found" : "NOT FOUND")}", Log.LogCategory.Systems);

            // Subscribe to mouse events
            if (middleClickAction != null) 
            {
                middleClickAction.performed += OnMiddleMousePressed;
                middleClickAction.canceled += OnMiddleMouseReleased;
                Log.Info("Camera: Middle click action bound successfully", Log.LogCategory.Systems);
            }
            else
            {
                Log.Error("Camera: Could not find UI/MiddleClick action", Log.LogCategory.Systems);
            }
            
            if (mousePositionAction == null)
            {
                Log.Error("Camera: Could not find UI/Point action", Log.LogCategory.Systems);
            }
            else
            {
                Log.Info("Camera: Mouse position action found successfully", Log.LogCategory.Systems);
            }
        }
        else
        {
            Log.Error("Camera: InputActions asset is null - attempting to reload from Resources", Log.LogCategory.Systems);
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            if (inputActions != null)
            {
                Log.Info("Camera: Successfully reloaded InputActions from Resources", Log.LogCategory.Systems);
                // Retry the setup
                OnEnable();
                return;
            }
            else
            {
                Log.Error("Camera: Failed to load InputActions from Resources", Log.LogCategory.Systems);
            }
        }
        
        // Test camera access immediately after setup
        Camera testCam = GetActiveCamera();
        Log.Info($"Camera: Active camera test after OnEnable: {(testCam != null ? testCam.name : "NONE FOUND")}", Log.LogCategory.Systems);
    }private void OnDisable()
    {
        // Unsubscribe from events
        if (middleClickAction != null) 
        {
            middleClickAction.performed -= OnMiddleMousePressed;
            middleClickAction.canceled -= OnMiddleMouseReleased;
        }
        
        // Disable the entire action asset
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }private void Update()
    {
        if (virtualCamera == null) return;

        HandleMovement();
        HandleMousePanning();
        HandleZoom();
    }private void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        bool hasKeyboardInput = false;
          // Get input from the Input System if available
        if (moveAction != null && moveAction.enabled)
        {
            input = moveAction.ReadValue<Vector2>();
            hasKeyboardInput = input.magnitude > 0.1f;
        }
        else
        {
            // No input if moveAction is not available
            Log.Debug("Camera: Move action not available", Log.LogCategory.Systems);
        }

        if (hasKeyboardInput)
        {
            // Calculate movement
            Vector3 direction = new Vector3(input.x, input.y, 0).normalized;
            targetPosition += direction * panSpeed * Time.deltaTime;

            // Clamp position to bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }        // Apply movement (whether from keyboard or mouse panning)
        Transform targetTransform = GetTargetTransform();
        if (targetTransform != null)
        {
            Vector3 oldPosition = targetTransform.position;
            if (smoothMovement)
            {
                targetTransform.position = Vector3.SmoothDamp(targetTransform.position, targetPosition, ref velocity, smoothTime);
            }
            else
            {
                targetTransform.position = targetPosition;
            }
            
            // Log when position actually changes
            if (Vector3.Distance(oldPosition, targetTransform.position) > 0.001f)
            {
                Log.Info($"Camera: Transform position updated from {oldPosition} to {targetTransform.position} (Target: {targetPosition}) on {targetTransform.name}", Log.LogCategory.Systems);
            }
        }
        else
        {
            Log.Warning("Camera: No target transform found for movement", Log.LogCategory.Systems);
        }
    }

    private void HandleZoom()
    {
        float zoomInput = 0f;
          // Get input from the Input System if available
        if (zoomAction != null && zoomAction.enabled)
        {
            Vector2 scroll = zoomAction.ReadValue<Vector2>();
            zoomInput = -scroll.y * 0.1f; // Scale down the scroll input
        }
        else
        {
            Log.Debug("Camera: Zoom action not available or not enabled", Log.LogCategory.Systems);
        }

        // Apply zoom
        targetZoom = Mathf.Clamp(targetZoom + zoomInput * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        #if CINEMACHINE_PRESENT
        virtualCamera.Lens.OrthographicSize = Mathf.Lerp(
            virtualCamera.Lens.OrthographicSize, 
            targetZoom, 
            Time.deltaTime * zoomSpeed
        );
        #else
        if (virtualCamera.orthographic)
        {            virtualCamera.orthographicSize = Mathf.Lerp(
                virtualCamera.orthographicSize,
                targetZoom,
                Time.deltaTime * zoomSpeed
            );
        }
        #endif
    }    private void HandleMousePanning()
    {
        // Debug: Log the state every few frames when mouse button should be released
        if (!isMiddleMousePressed && Time.frameCount % 30 == 0)
        {
            Log.Debug($"Camera: Mouse panning state check - isPressed: {isMiddleMousePressed}, middleClickAction active: {(middleClickAction != null && middleClickAction.IsPressed())}", Log.LogCategory.Systems);
        }
        
        if (isMiddleMousePressed && mousePositionAction != null)
        {
            // Additional safety check: verify the middle click action is actually still pressed
            if (middleClickAction != null && !middleClickAction.IsPressed())
            {
                Log.Warning("Camera: Middle click action reports not pressed but isMiddleMousePressed is true - forcing release", Log.LogCategory.Systems);
                OnMiddleMouseReleased(new InputAction.CallbackContext());
                return;
            }
            
            Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
            
            // Get the appropriate camera for screen to world conversion
            Camera cam = GetActiveCamera();
            if (cam == null) 
            {
                Log.Warning("Camera: Cannot handle mouse panning - no active camera", Log.LogCategory.Systems);
                return;
            }
            
            // Use camera's transform.position.z for consistent world positioning
            Vector3 currentMouseWorldPos = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -cam.transform.position.z));
            
            if (lastMousePosition != Vector3.zero)
            {
                Vector3 mouseDelta = lastMousePosition - currentMouseWorldPos;
                
                // Apply sensitivity, speed, and responsiveness settings
                float deltaX = mouseDelta.x * mousePanSpeed * mousePanSensitivity * mousePanResponsiveness * Time.deltaTime;
                float deltaY = mouseDelta.y * mousePanSpeed * mousePanSensitivity * mousePanResponsiveness * Time.deltaTime;
                
                if (invertMousePanX) deltaX = -deltaX;
                if (invertMousePanY) deltaY = -deltaY;
                
                Vector3 oldTargetPosition = targetPosition;
                targetPosition += new Vector3(deltaX, deltaY, 0);
                
                // Clamp position to bounds
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
                
                Log.Debug($"Camera: Mouse panning active - Delta applied: ({deltaX:F3}, {deltaY:F3})", Log.LogCategory.Systems);
            }
            
            lastMousePosition = currentMouseWorldPos;
        }
    }private Camera GetActiveCamera()
    {
        Camera cam = null;
        
        // Try multiple approaches to find a working camera
        
        #if CINEMACHINE_PRESENT
        if (virtualCamera != null && virtualCamera.enabled)
        {
            // Method 1: Try to find the camera brain
            var brain = FindFirstObjectByType<CinemachineBrain>();
            if (brain != null)
            {
                cam = brain.GetComponent<Camera>();
                if (cam != null)
                {
                    Log.Debug($"Camera: Using Cinemachine brain camera: {cam.name}", Log.LogCategory.Systems);
                    return cam;
                }
            }
            
            // Method 2: Look for camera in virtualCamera's GameObject
            cam = virtualCamera.GetComponent<Camera>();
            if (cam != null)
            {
                Log.Debug($"Camera: Using Cinemachine direct camera: {cam.name}", Log.LogCategory.Systems);
                return cam;
            }
            
            // Method 3: Look in parent/children
            cam = virtualCamera.GetComponentInParent<Camera>();
            if (cam != null)
            {
                Log.Debug($"Camera: Using Cinemachine parent camera: {cam.name}", Log.LogCategory.Systems);
                return cam;
            }
            
            cam = virtualCamera.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                Log.Debug($"Camera: Using Cinemachine child camera: {cam.name}", Log.LogCategory.Systems);
                return cam;
            }
        }
        #else
        if (virtualCamera != null)
        {
            cam = virtualCamera as Camera;
            if (cam != null)
            {
                Log.Debug($"Camera: Using direct camera reference: {cam.name}", Log.LogCategory.Systems);
                return cam;
            }
        }
        #endif
        
        // Method 4: Try Camera.main
        cam = Camera.main;
        if (cam != null)
        {
            Log.Debug($"Camera: Using Camera.main: {cam.name}", Log.LogCategory.Systems);
            return cam;
        }
        
        // Method 5: Find any camera in the scene
        cam = FindFirstObjectByType<Camera>();
        if (cam != null)
        {
            Log.Debug($"Camera: Using first found camera: {cam.name}", Log.LogCategory.Systems);
            return cam;
        }
        
        // Method 6: Check if this GameObject has a camera
        cam = GetComponent<Camera>();
        if (cam != null)
        {
            Log.Debug($"Camera: Using CameraManager's own camera: {cam.name}", Log.LogCategory.Systems);
            return cam;
        }
        
        Log.Error("Camera: No camera found in scene for mouse panning! Please ensure there is an active camera in the scene.", Log.LogCategory.Systems);
        return null;
    }    private void OnMiddleMousePressed(InputAction.CallbackContext context)
    {
        Log.Info("Camera: OnMiddleMousePressed triggered", Log.LogCategory.Systems);
        
        // First check if we can get a camera before proceeding
        Camera cam = GetActiveCamera();
        if (cam == null)
        {
            Log.Error("Camera: Cannot start mouse panning - no active camera found!", Log.LogCategory.Systems);
            return;
        }
        
        Log.Info($"Camera: Using camera for mouse panning: {cam.name}", Log.LogCategory.Systems);
        
        isMiddleMousePressed = true;
        if (mousePositionAction != null)
        {
            Vector2 mousePosition = mousePositionAction.ReadValue<Vector2>();
            
            // Use camera's transform.position.z for consistent world positioning
            lastMousePosition = cam.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -cam.transform.position.z));
            mousePanStartPosition = targetPosition;
            
            Log.Info($"Camera: Started middle mouse panning at mouse position: {mousePosition}, world position: {lastMousePosition}", Log.LogCategory.Systems);
        }
        else
        {
            Log.Error("Camera: Mouse position action is null - cannot determine mouse position", Log.LogCategory.Systems);
            isMiddleMousePressed = false; // Reset if we can't get mouse position
        }
    }    private void OnMiddleMouseReleased(InputAction.CallbackContext context)
    {
        Log.Info($"Camera: OnMiddleMouseReleased called - was pressed: {isMiddleMousePressed}, context phase: {context.phase}", Log.LogCategory.Systems);
        
        if (isMiddleMousePressed)
        {
            Log.Info("Camera: Stopped middle mouse panning", Log.LogCategory.Systems);
            
            // If immediate stop is enabled, snap camera to current position to stop smooth movement
            if (immediateStopOnMouseRelease)
            {
                Transform targetTransform = GetTargetTransform();
                if (targetTransform != null)
                {
                    targetPosition = targetTransform.position;
                    Log.Info($"Camera: Immediate stop - Set target position to current position: {targetPosition}", Log.LogCategory.Systems);
                }
            }
        }
        
        isMiddleMousePressed = false;
        lastMousePosition = Vector3.zero;
        
        Log.Info($"Camera: Mouse panning state after release - isPressed: {isMiddleMousePressed}", Log.LogCategory.Systems);
    }

    /// <summary>
    /// Focus camera on a specific transform
    /// </summary>
    /// <param name="target">Transform to focus on</param>
    public void FocusOnTransform(Transform target)
    {
        if (target == null) return;

        targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        
        // Clamp position to bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
    }

    private void OnDestroy()
    {        if (Instance == this)
        {
            Instance = null;
        }
    }
    
    #region Debug Methods
    
    /// <summary>
    /// Manual debug method to test camera detection - call from Unity console or other scripts
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugTestCameraDetection()
    {
        Log.Info("=== CAMERA DETECTION DEBUG TEST ===", Log.LogCategory.Systems);
        
        Camera cam = GetActiveCamera();
        Log.Info($"Result: {(cam != null ? $"SUCCESS - Found camera: {cam.name}" : "FAILED - No camera found")}", Log.LogCategory.Systems);
        
        if (cam != null)
        {
            Log.Info($"Camera details: Position: {cam.transform.position}, Rotation: {cam.transform.rotation}", Log.LogCategory.Systems);
            Log.Info($"Camera settings: Orthographic: {cam.orthographic}, Size/FOV: {(cam.orthographic ? cam.orthographicSize.ToString() : cam.fieldOfView.ToString())}", Log.LogCategory.Systems);
        }
        
        // Test input actions
        Log.Info($"InputActions: {(inputActions != null ? "LOADED" : "NULL")}", Log.LogCategory.Systems);
        Log.Info($"MiddleClick Action: {(middleClickAction != null ? "FOUND" : "NULL")}", Log.LogCategory.Systems);
        Log.Info($"MousePosition Action: {(mousePositionAction != null ? "FOUND" : "NULL")}", Log.LogCategory.Systems);
        Log.Info($"Move Action: {(moveAction != null ? "FOUND" : "NULL")}", Log.LogCategory.Systems);
        
        Log.Info("=== END DEBUG TEST ===", Log.LogCategory.Systems);
    }
    
    /// <summary>
    /// Test method to simulate middle mouse press - for debugging purposes
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugTestMousePanning()
    {
        Log.Info("=== MOUSE PANNING DEBUG TEST ===", Log.LogCategory.Systems);
        
        if (mousePositionAction == null)
        {
            Log.Error("Cannot test mouse panning - mousePositionAction is null", Log.LogCategory.Systems);
            return;
        }
        
        Vector2 mousePos = mousePositionAction.ReadValue<Vector2>();
        Log.Info($"Current mouse position: {mousePos}", Log.LogCategory.Systems);
        
        Camera cam = GetActiveCamera();
        if (cam != null)
        {
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -cam.transform.position.z));
            Log.Info($"Mouse world position: {worldPos}", Log.LogCategory.Systems);
        }
        else
        {
            Log.Error("Cannot convert mouse position - no camera available", Log.LogCategory.Systems);
        }
        
        Log.Info("=== END MOUSE PANNING TEST ===", Log.LogCategory.Systems);
    }
    
    /// <summary>
    /// Debug method to test camera movement manually
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugTestMovement(Vector3 offset)
    {
        Log.Info($"=== MOVEMENT DEBUG TEST - Offset: {offset} ===", Log.LogCategory.Systems);
        
        Transform targetTransform = GetTargetTransform();
        if (targetTransform != null)
        {
            Vector3 oldPosition = targetTransform.position;
            targetPosition += offset;
            
            // Apply immediately for testing
            targetTransform.position = targetPosition;
            
            Log.Info($"Camera: Moved {targetTransform.name} from {oldPosition} to {targetTransform.position}", Log.LogCategory.Systems);
        }
        else
        {
            Log.Error("Camera: Cannot test movement - no target transform found", Log.LogCategory.Systems);
        }
        
        Log.Info("=== END MOVEMENT TEST ===", Log.LogCategory.Systems);
    }
    
    /// <summary>
    /// Apply fast and responsive mouse panning settings
    /// </summary>
    [ContextMenu("Apply Fast Panning Preset")]
    public void ApplyFastPanningPreset()
    {
        mousePanSpeed = 2f;
        mousePanSensitivity = 2f;
        mousePanResponsiveness = 10f;
        immediateStopOnMouseRelease = true;
        smoothMovement = false;
        Log.Info("Camera: Applied Fast Panning Preset", Log.LogCategory.Systems);
    }
    
    /// <summary>
    /// Apply smooth and cinematic mouse panning settings
    /// </summary>
    [ContextMenu("Apply Smooth Panning Preset")]
    public void ApplySmoothPanningPreset()
    {
        mousePanSpeed = 1f;
        mousePanSensitivity = 1f;
        mousePanResponsiveness = 3f;
        immediateStopOnMouseRelease = false;
        smoothMovement = true;
        smoothTime = 0.3f;
        Log.Info("Camera: Applied Smooth Panning Preset", Log.LogCategory.Systems);
    }
    
    /// <summary>
    /// Apply responsive mouse panning settings (recommended for most games)
    /// </summary>
    [ContextMenu("Apply Responsive Panning Preset")]
    public void ApplyResponsivePanningPreset()
    {
        mousePanSpeed = 1.5f;
        mousePanSensitivity = 1.5f;
        mousePanResponsiveness = 7f;
        immediateStopOnMouseRelease = true;
        smoothMovement = true;
        smoothTime = 0.1f;
        Log.Info("Camera: Applied Responsive Panning Preset", Log.LogCategory.Systems);
    }
    
    /// <summary>
    /// Debug method to check current mouse state
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugCheckMouseState()
    {
        Log.Info("=== MOUSE STATE DEBUG ===", Log.LogCategory.Systems);
        Log.Info($"isMiddleMousePressed: {isMiddleMousePressed}", Log.LogCategory.Systems);
        Log.Info($"Legacy Input GetMouseButton(2): {Input.GetMouseButton(2)}", Log.LogCategory.Systems);
        Log.Info($"MiddleClick Action IsPressed: {(middleClickAction != null ? middleClickAction.IsPressed().ToString() : "NULL")}", Log.LogCategory.Systems);
        Log.Info($"MiddleClick Action Enabled: {(middleClickAction != null ? middleClickAction.enabled.ToString() : "NULL")}", Log.LogCategory.Systems);
        Log.Info($"Last Mouse Position: {lastMousePosition}", Log.LogCategory.Systems);
        Log.Info("=== END MOUSE STATE DEBUG ===", Log.LogCategory.Systems);
    }
    
    #endregion

    /// <summary>
    /// Gets the transform that should be moved for camera panning
    /// </summary>
    private Transform GetTargetTransform()
    {
        #if CINEMACHINE_PRESENT
        // If using Cinemachine, move the virtual camera's transform
        if (virtualCamera != null)
        {
            return virtualCamera.transform;
        }
        #else
        // If using regular camera, move the camera's transform
        if (virtualCamera != null)
        {
            return virtualCamera.transform;
        }
        #endif
        
        // Fallback to this GameObject's transform
        return transform;
    }
}
}
