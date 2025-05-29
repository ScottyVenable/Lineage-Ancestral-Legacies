using UnityEngine;
using Unity.Cinemachine;

namespace Lineage.Ancestral.Legacies.Managers
{
    /// <summary>
    /// Manages RTS-style camera controls, following selected units, and zoom.
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [Header("Camera Settings")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 20f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private bool smoothMovement = true;
        [SerializeField] private float smoothTime = 0.5f;

        [Header("Camera Bounds")]
        [SerializeField] private Vector2 minBounds = new Vector2(-50, -50);
        [SerializeField] private Vector2 maxBounds = new Vector2(50, 50);

        [Header("Cinemachine")]
        [SerializeField] private CinemachineCamera virtualCamera;

        private Camera mainCamera;
        private Transform cameraTransform;
        private Vector3 targetPosition;
        private Transform followTarget;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeCamera();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Subscribe to selection events
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged += OnSelectionChanged;
            }
        }

        private void OnDestroy()
        {
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.OnSelectionChanged -= OnSelectionChanged;
            }
        }

        private void InitializeCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }

            if (virtualCamera == null)
            {
                virtualCamera = FindFirstObjectByType<CinemachineCamera>();
            }

            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                targetPosition = cameraTransform.position;
            }
            else
            {
                UnityEngine.Debug.LogError("CameraManager: No camera found in scene!");
            }
        }

        private void Update()
        {
            if (mainCamera == null) return;

            HandleInput();
            UpdateCameraPosition();
        }

        private void HandleInput()
        {
            // Pan camera with WASD or arrow keys
            Vector3 movement = Vector3.zero;
            
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                movement += Vector3.forward;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                movement += Vector3.back;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                movement += Vector3.left;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                movement += Vector3.right;

            // Pan with mouse at screen edges
            Vector3 mousePosition = Input.mousePosition;
            float edgeThreshold = 10f;
            
            if (mousePosition.x < edgeThreshold)
                movement += Vector3.left;
            if (mousePosition.x > Screen.width - edgeThreshold)
                movement += Vector3.right;
            if (mousePosition.y < edgeThreshold)
                movement += Vector3.back;
            if (mousePosition.y > Screen.height - edgeThreshold)
                movement += Vector3.forward;

            if (movement != Vector3.zero)
            {
                // Stop following when manually panning
                followTarget = null;
                MoveCamera(movement.normalized * panSpeed * Time.deltaTime);
            }

            // Zoom with scroll wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ZoomCamera(-scroll * zoomSpeed);
            }

            // Rotate camera with Q/E
            if (Input.GetKey(KeyCode.Q))
                RotateCamera(-rotationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.E))
                RotateCamera(rotationSpeed * Time.deltaTime);

            // Focus on selection with F key
            if (Input.GetKeyDown(KeyCode.F))
            {
                FocusOnSelection();
            }
        }

        private void OnSelectionChanged()
        {
            // Optionally auto-follow selected units
            if (SelectionManager.Instance != null && SelectionManager.Instance.HasSelection())
            {
                var selectedPops = SelectionManager.Instance.GetSelectedPops();
                if (selectedPops.Count > 0)
                {
                    FocusOnTransform(selectedPops[0].transform);
                }
            }
        }

        private void UpdateCameraPosition()
        {
            if (followTarget != null)
            {
                // Follow the target
                Vector3 desiredPosition = followTarget.position + new Vector3(0, 10, -10);
                targetPosition = desiredPosition;
            }

            // Clamp position to bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, minBounds.y, maxBounds.y);

            // Apply smooth movement
            if (smoothMovement)
            {
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, Time.deltaTime / smoothTime);
            }
            else
            {
                cameraTransform.position = targetPosition;
            }
        }

        public void MoveCamera(Vector3 movement)
        {
            targetPosition += movement;
        }

        public void ZoomCamera(float zoomAmount)
        {
            if (virtualCamera != null)
            {
                var lens = virtualCamera.Lens;
                lens.FieldOfView = Mathf.Clamp(lens.FieldOfView + zoomAmount, minZoom, maxZoom);
                virtualCamera.Lens = lens;
            }
            else if (mainCamera != null)
            {
                mainCamera.fieldOfView = Mathf.Clamp(mainCamera.fieldOfView + zoomAmount, minZoom, maxZoom);
            }
        }

        public void RotateCamera(float rotationAmount)
        {
            cameraTransform.Rotate(0, rotationAmount, 0);
        }

        public void FocusOnTransform(Transform target)
        {
            if (target != null)
            {
                followTarget = target;
                targetPosition = target.position + new Vector3(0, 10, -10);

                if (virtualCamera != null)
                {
                    virtualCamera.Follow = target;
                    virtualCamera.LookAt = target;
                }
            }
        }

        public void FocusOnSelection()
        {
            if (SelectionManager.Instance != null && SelectionManager.Instance.HasSelection())
            {
                var selectedPops = SelectionManager.Instance.GetSelectedPops();
                if (selectedPops.Count > 0)
                {
                    FocusOnTransform(selectedPops[0].transform);
                }
            }
        }

        public void FocusOnPosition(Vector3 position)
        {
            followTarget = null;
            targetPosition = position + new Vector3(0, 10, -10);

            if (virtualCamera != null)
            {
                virtualCamera.Follow = null;
                virtualCamera.LookAt = null;
            }
        }

        public void SetCameraBounds(Vector2 minBounds, Vector2 maxBounds)
        {
            this.minBounds = minBounds;
            this.maxBounds = maxBounds;
        }

        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            if (virtualCamera != null)
            {
                virtualCamera.Follow = target;
                virtualCamera.LookAt = target;
            }
        }

        public void StopFollowing()
        {
            followTarget = null;
            if (virtualCamera != null)
            {
                virtualCamera.Follow = null;
                virtualCamera.LookAt = null;
            }
        }
    }
}
