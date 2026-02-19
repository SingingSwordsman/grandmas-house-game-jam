using UnityEngine;

public class LockedCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The point the camera looks at. Leave empty to auto-create one at scene origin.")]
    public Transform target;

    [Header("Pan Settings")]
    public float panSpeed = 5f;
    public float minHorizontalOffset = -5f;
    public float maxHorizontalOffset = 5f;
    public float panSmoothTime = 0.1f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minDistance = 3f;
    public float maxDistance = 15f;
    public float zoomSmoothTime = 0.1f;

    [Header("Initial State")]
    public float initialDistance = 8f;
    public float initialHorizontalOffset = 0f;
    public float fixedVerticalAngle = 20f;   // Static up/down look angle
    public float fixedHorizontalAngle = 0f;  // Static left/right facing direction

    // ── Private state ─────────────────────────────────────────────────────────
    private float _targetDistance;
    private float _currentDistance;
    private float _zoomVelocity;

    private float _targetHorizontalOffset;
    private float _currentHorizontalOffset;
    private float _panVelocity;

    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (target == null)
        {
            GameObject pivot = new GameObject("CameraTarget");
            target = pivot.transform;
        }

        _targetDistance = initialDistance;
        _currentDistance = initialDistance;
        _targetHorizontalOffset = initialHorizontalOffset;
        _currentHorizontalOffset = initialHorizontalOffset;

        ApplyTransform();
    }

    private void LateUpdate()
    {
        if (Selectable.IsDragging == true) return;

        HandlePanInput();
        HandleZoomInput();
        SmoothValues();
        ApplyTransform();
    }

    // ── Input ─────────────────────────────────────────────────────────────────

    private void HandlePanInput()
    {
        // Pan on: Middle Mouse drag  OR  Right Mouse drag  OR  Alt + Left Mouse drag
        bool panning = Input.GetMouseButton(2)
                    || Input.GetMouseButton(1)
                    || (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt));

        if (!panning) return;

        float mouseX = Input.GetAxis("Mouse X");
        _targetHorizontalOffset -= mouseX * panSpeed * 0.1f;
        _targetHorizontalOffset = Mathf.Clamp(_targetHorizontalOffset, minHorizontalOffset, maxHorizontalOffset);
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

    private void SmoothValues()
    {
        _currentDistance = Mathf.SmoothDamp(
            _currentDistance, _targetDistance,
            ref _zoomVelocity, zoomSmoothTime);

        _currentHorizontalOffset = Mathf.SmoothDamp(
            _currentHorizontalOffset, _targetHorizontalOffset,
            ref _panVelocity, panSmoothTime);
    }

    private void ApplyTransform()
    {
        // Fixed rotation — no orbiting
        Quaternion rotation = Quaternion.Euler(fixedVerticalAngle, fixedHorizontalAngle, 0f);

        // Base position behind the target
        Vector3 baseOffset = rotation * new Vector3(0f, 0f, -_currentDistance);

        // Horizontal pan is applied in world-space right relative to our fixed facing direction
        Vector3 right = rotation * Vector3.right;
        Vector3 panOffset = right * _currentHorizontalOffset;

        transform.position = target.position + baseOffset + panOffset;
        transform.LookAt(target.position + panOffset); // look toward the panned point so framing feels natural
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void ResetCamera()
    {
        _targetDistance = initialDistance;
        _currentDistance = initialDistance;
        _targetHorizontalOffset = initialHorizontalOffset;
        _currentHorizontalOffset = initialHorizontalOffset;
        ApplyTransform();
    }

    public void SetTarget(Transform newTarget) => target = newTarget;
}