using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if CINEMACHINE_PRESENT
using Cinemachine;
#endif
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
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
    public CinemachineVirtualCamera virtualCamera;
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
        // Initialize if references are missing
        #if CINEMACHINE_PRESENT
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            Debug.LogWarning("VirtualCamera reference not set - found in scene: " + (virtualCamera != null));
        }
        #else
        if (virtualCamera == null)
        {
            virtualCamera = Camera.main;
            Debug.LogWarning("Cinemachine not found. Using standard Camera.main as fallback.");
        }
        #endif

        // Load the input actions if not set
        if (inputActions == null)
        {
            inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            Debug.LogWarning("InputActions reference not set - attempting to load from Resources");
        }

        // Initialize positions
        if (virtualCamera != null)
        {
            targetPosition = transform.position;
            #if CINEMACHINE_PRESENT
            targetZoom = virtualCamera.m_Lens.OrthographicSize;
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
        virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(
            virtualCamera.m_Lens.OrthographicSize, 
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
}
