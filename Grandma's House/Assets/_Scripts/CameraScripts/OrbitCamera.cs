using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The point the camera orbits around. Leave empty to auto-create one at scene origin.")]
    public Transform target;

    [Header("Orbit Settings")]
    public float orbitSpeed = 5f;
    [Range(-89f, 0f)] public float minVerticalAngle = -80f;
    [Range(0f, 89f)] public float maxVerticalAngle = 80f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minDistance = 1f;
    public float maxDistance = 20f;
    public float smoothTime = 0.1f;   // Smoothing for zoom

    [Header("Initial State")]
    public float initialDistance = 8f;
    public float initialHorizontalAngle = 45f;
    public float initialVerticalAngle = 30f;

    // ── Private state ─────────────────────────────────────────────────────────
    private float _horizontalAngle;
    private float _verticalAngle;
    private float _currentDistance;
    private float _targetDistance;
    private float _zoomVelocity;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Auto-create a target at the world origin if none assigned
        if (target == null)
        {
            GameObject pivot = new GameObject("CameraOrbitTarget");
            target = pivot.transform;
        }

        _horizontalAngle = initialHorizontalAngle;
        _verticalAngle = initialVerticalAngle;
        _currentDistance = initialDistance;
        _targetDistance = initialDistance;

        ApplyTransform();
    }

    private void LateUpdate()
    {
        if (Selectable.IsDragging == true) return;

        HandleOrbitInput();
        HandleZoomInput();
        SmoothZoom();
        ApplyTransform();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandleOrbitInput()
    {
        // Orbit on: Middle Mouse drag  OR  Right Mouse drag  OR  Alt + Left Mouse drag
        bool orbiting = Input.GetMouseButton(2)
                     || Input.GetMouseButton(1)
                     || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt));

        if (!orbiting) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _horizontalAngle += mouseX * orbitSpeed;
        _verticalAngle -= mouseY * orbitSpeed;   // invert Y so dragging up tilts up

        _verticalAngle = Mathf.Clamp(_verticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    private void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            _targetDistance -= scroll * zoomSpeed;
            _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
        }
    }

    private void SmoothZoom()
    {
        _currentDistance = Mathf.SmoothDamp(
            _currentDistance, _targetDistance,
            ref _zoomVelocity, smoothTime);
    }

    // ── Transform ─────────────────────────────────────────────────────────────

    private void ApplyTransform()
    {
        // Build a rotation from the two angles
        Quaternion rotation = Quaternion.Euler(_verticalAngle, _horizontalAngle, 0f);

        // Position the camera behind and above the target by _currentDistance
        Vector3 offset = rotation * new Vector3(0f, 0f, -_currentDistance);
        transform.position = target.position + offset;

        // Always look at the target
        transform.LookAt(target.position);
    }


    public void ResetCamera()
    {
        _horizontalAngle = initialHorizontalAngle;
        _verticalAngle = initialVerticalAngle;
        _targetDistance = initialDistance;
        _currentDistance = initialDistance;
        ApplyTransform();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
