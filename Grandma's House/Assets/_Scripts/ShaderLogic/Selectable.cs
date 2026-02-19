using UnityEngine;
using System.Collections.Generic;

public class Selectable : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Vector3 boundsMin;
    [SerializeField] private Vector3 boundsMax;
    [SerializeField] private float dragStrength = 50f;
    [SerializeField] private float damping = 10f;
    [SerializeField] private float depthMoveSpeed = 5f;

    public static bool IsDragging { get; private set; }

    private Renderer _renderer;
    private Material[] _defaultMaterials;
    private Rigidbody _rb;
    private Camera _cam;

    private bool _dragging;
    private Vector3 _worldTarget;  // target position in world space directly
    private Plane _dragPlane;       // plane we raycast against to get world position

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _defaultMaterials = _renderer.materials;
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
    }

    void OnMouseEnter()
    {
        if (_dragging) return;
        AddOutline();
    }

    void OnMouseExit()
    {
        if (_dragging) return;
        RemoveOutline();
    }

    void OnMouseDown()
    {
        _dragging = true;
        IsDragging = true;
        _rb.useGravity = false;

        // Create a drag plane at the object's current position facing the camera
        _dragPlane = new Plane(-_cam.transform.forward, transform.position);
        _worldTarget = transform.position;

        AddOutline();
    }

    void OnMouseUp()
    {
        _dragging = false;
        IsDragging = false;
        _rb.useGravity = true;
        RemoveOutline();
    }

    void Update()
    {
        if (!_dragging) return;

        // Move the drag plane closer or further from camera with W/S
        if (Input.GetKey(KeyCode.W))
            _worldTarget += _cam.transform.forward * depthMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            _worldTarget -= _cam.transform.forward * depthMoveSpeed * Time.deltaTime;

        // Raycast from mouse onto the drag plane to get world position
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (_dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorld = ray.GetPoint(distance);

            // Get current target in camera local space
            Vector3 localTarget = _cam.transform.InverseTransformPoint(_worldTarget);
            Vector3 localMouse = _cam.transform.InverseTransformPoint(mouseWorld);

            // Take XY from mouse but keep Z (depth) from W/S input
            localTarget.x = localMouse.x;
            localTarget.y = localMouse.y;

            // Convert back to world space
            _worldTarget = _cam.transform.TransformPoint(localTarget);

            _dragPlane = new Plane(-_cam.transform.forward, _worldTarget);
        }

        // Clamp target to bounds BEFORE applying force so physics never fights a clamped position
        _worldTarget = new Vector3(
            Mathf.Clamp(_worldTarget.x, boundsMin.x, boundsMax.x),
            Mathf.Clamp(_worldTarget.y, boundsMin.y, boundsMax.y),
            Mathf.Clamp(_worldTarget.z, boundsMin.z, boundsMax.z)
        );
    }

    void FixedUpdate()
    {
        if (_dragging)
        {
            Vector3 direction = _worldTarget - transform.position;
            _rb.linearVelocity = direction * dragStrength * Time.fixedDeltaTime * 60f;
            _rb.linearVelocity *= (1f - damping * Time.fixedDeltaTime);
        }

        // Always clamp position regardless of dragging or physics velocity
        Vector3 pos = transform.position;
        Vector3 vel = _rb.linearVelocity;

        if (pos.x < boundsMin.x) { pos.x = boundsMin.x; vel.x = Mathf.Max(0, vel.x); }
        if (pos.x > boundsMax.x) { pos.x = boundsMax.x; vel.x = Mathf.Min(0, vel.x); }
        if (pos.y < boundsMin.y) { pos.y = boundsMin.y; vel.y = Mathf.Max(0, vel.y); }
        if (pos.y > boundsMax.y) { pos.y = boundsMax.y; vel.y = Mathf.Min(0, vel.y); }
        if (pos.z < boundsMin.z) { pos.z = boundsMin.z; vel.z = Mathf.Max(0, vel.z); }
        if (pos.z > boundsMax.z) { pos.z = boundsMax.z; vel.z = Mathf.Min(0, vel.z); }

        _rb.linearVelocity = vel;
        transform.position = pos;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.2f);
        Vector3 center = (boundsMin + boundsMax) / 2f;
        Vector3 size = boundsMax - boundsMin;
        Gizmos.DrawCube(center, size);
        Gizmos.color = new Color(1, 1, 0, 1f);
        Gizmos.DrawWireCube(center, size);
    }

    void AddOutline()
    {
        var mats = new List<Material>(_defaultMaterials) { outlineMaterial };
        _renderer.materials = mats.ToArray();
    }

    void RemoveOutline()
    {
        _renderer.materials = _defaultMaterials;
    }
}