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

    public Vector3 BoundsMin => boundsMin;
    public Vector3 BoundsMax => boundsMax;

    public static bool IsDragging { get; private set; }

    private Renderer _renderer;
    private Material[] _defaultMaterials;
    private Rigidbody _rb;
    private Camera _cam;

    private bool _dragging;
    private bool _inventoryDrag;
    private Vector3 _worldTarget;
    private Plane _dragPlane;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _defaultMaterials = _renderer.materials;
        _rb = GetComponent<Rigidbody>();
        _cam = Camera.main;
    }

    // ── Normal mouse interaction ───────────────────────────────────
    void OnMouseEnter() { if (!_dragging) AddOutline(); }
    void OnMouseExit() { if (!_dragging) RemoveOutline(); }

    void OnMouseDown()
    {
        if (_inventoryDrag) return;
        StartDrag(transform.position);
    }

    void OnMouseUp()
    {
        if (!_dragging) return;
        _inventoryDrag = false;
        StopDrag();
    }

    // ── Called by InventoryPanel right after Instantiate ──────────
    public void BeginDragFromInventory(Vector3 spawnPos)
    {
        _inventoryDrag = true;
        StartDrag(spawnPos);
    }

    // ── Shared drag start ──────────────────────────────────────────
    void StartDrag(Vector3 startWorldPos)
    {
        _dragging = true;
        IsDragging = true;
        _rb.useGravity = false;

        _worldTarget = startWorldPos;
        _dragPlane = new Plane(-_cam.transform.forward, startWorldPos);

        AddOutline();
    }

    void StopDrag()
    {
        _dragging = false;
        IsDragging = false;
        _rb.useGravity = true;
        RemoveOutline();
    }

    // ── Every frame while dragging ─────────────────────────────────
    void Update()
    {
        if (!_dragging) return;

        if (Input.GetKey(KeyCode.W))
            _worldTarget += _cam.transform.forward * depthMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            _worldTarget -= _cam.transform.forward * depthMoveSpeed * Time.deltaTime;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        if (_dragPlane.Raycast(ray, out float distance))
        {
            Vector3 mouseWorld = ray.GetPoint(distance);
            Vector3 localTarget = _cam.transform.InverseTransformPoint(_worldTarget);
            Vector3 localMouse = _cam.transform.InverseTransformPoint(mouseWorld);

            localTarget.x = localMouse.x;
            localTarget.y = localMouse.y;

            _worldTarget = _cam.transform.TransformPoint(localTarget);
            _dragPlane = new Plane(-_cam.transform.forward, _worldTarget);
        }

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