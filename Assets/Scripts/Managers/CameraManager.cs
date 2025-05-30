using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CINEMACHINE_PRESENT
using Unity.Cinemachine;
#endif
using UnityEngine.InputSystem;
using Lineage.Ancestral.Legacies.Debug;

namespace Lineage.Ancestral.Legacies.Managers
{
    public class CameraManager : MonoBehaviour
{
    // Singleton instance
    public static CameraManager Instance { get; private set; }

    [Header("Camera Settings")]
    public float panSpeed = 20f;
    public float zoomSpeed = 10f;
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float rotationSpeed = 100f;
    public bool smoothMovement = true;
    public float smoothTime = 0.5f;

    [Header("Boundaries")]
    public Vector2 minBounds = new Vector2(-50f, -50f);
    public Vector2 maxBounds = new Vector2(50f, 50f);

    [Header("References")]
    #if CINEMACHINE_PRESENT
    public CinemachineCamera virtualCamera;
    #else
    public Camera virtualCamera;
    #endif
    public InputActionAsset inputActions;

    // Private variables
    private InputAction moveAction;
    private InputAction zoomAction;
    private Vector3 targetPosition;
    private float targetZoom;
    private Vector3 velocity = Vector3.zero;

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
        }

        // Initialize if references are missing
        #if CINEMACHINE_PRESENT
        if (virtualCamera == null)
        {
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            Log.Warning("VirtualCamera reference not set - found in scene: " + (virtualCamera != null), Log.LogCategory.Systems);
        }
        #else
        if (virtualCamera == null)
        {
            virtualCamera = Camera.main;
            Log.Warning("Cinemachine not found. Using standard Camera.main as fallback.", Log.LogCategory.Systems);
        }
        #endif

        // Load the input actions if not set
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            Log.Warning("InputActions reference not set - attempting to load from Resources", Log.LogCategory.Systems);
        }

        // Initialize positions
        if (virtualCamera != null)
        {
            targetPosition = transform.position;
            #if CINEMACHINE_PRESENT
            targetZoom = virtualCamera.Lens.OrthographicSize;
            #else
            if (virtualCamera.orthographic)
                targetZoom = virtualCamera.orthographicSize;
            else
                targetZoom = minZoom;
            #endif
        }
    }

    private void OnEnable()
    {
        if (inputActions != null)
        {
            // Enable input actions
            moveAction = inputActions.FindAction("Player/Move");
            zoomAction = inputActions.FindAction("UI/ScrollWheel");

            if (moveAction != null) moveAction.Enable();
            if (zoomAction != null) zoomAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.Disable();
        if (zoomAction != null) zoomAction.Disable();
    }

    private void Update()
    {
        if (virtualCamera == null) return;

        HandleMovement();
        HandleZoom();
    }

    private void HandleMovement()
    {
        Vector2 input = Vector2.zero;
        
        // Get input from the Input System if available
        if (moveAction != null && moveAction.enabled)
        {
            input = moveAction.ReadValue<Vector2>();
        }
        // Fallback to legacy input system
        else
        {
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
        }

        // Calculate movement
        Vector3 direction = new Vector3(input.x, input.y, 0).normalized;
        targetPosition += direction * panSpeed * Time.deltaTime;

        // Clamp position to bounds
        targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

        // Apply movement
        if (smoothMovement)
        {
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    private void HandleZoom()
    {
        // Check if mouse is over debug console and skip zoom if so
        var debugConsole = FindFirstObjectByType<Lineage.Ancestral.Legacies.Debug.DebugConsoleManager>();
        if (debugConsole != null && debugConsole.IsMouseOverConsole())
        {
            return;
        }

        float zoomInput = 0f;
        
        // Get input from the Input System if available
        if (zoomAction != null && zoomAction.enabled)
        {
            Vector2 scroll = zoomAction.ReadValue<Vector2>();
            zoomInput = -scroll.y * 0.1f; // Scale down the scroll input
        }
        // Fallback to legacy input system
        else if (Input.mouseScrollDelta.y != 0)
        {
            zoomInput = -Input.mouseScrollDelta.y;
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
        {
            virtualCamera.orthographicSize = Mathf.Lerp(
                virtualCamera.orthographicSize,
                targetZoom,
                Time.deltaTime * zoomSpeed
            );
        }
        #endif
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
}
}
