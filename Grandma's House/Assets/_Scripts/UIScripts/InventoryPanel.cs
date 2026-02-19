using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryPanel : MonoBehaviour
{
    [Header("UI References")]
    public ScrollRect scrollRect;
    public RectTransform contentParent;
    public GameObject inventorySlotPrefab;

    [Header("Inventory Prefabs")]
    public List<GameObject> inventoryItems = new List<GameObject>();

    // ── Panel slide ────────────────────────────────────────────────
    private RectTransform _panelRect;
    private bool _panelVisible = true;
    private float _shownX, _hiddenX;

    private Camera _cam;

    void Awake()
    {
        _cam = Camera.main;
        _panelRect = GetComponent<RectTransform>();
        _shownX = _panelRect.anchoredPosition.x;
        _hiddenX = _shownX + _panelRect.rect.width;

        BuildSlots();
    }

    void BuildSlots()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        for (int i = 0; i < inventoryItems.Count; i++)
        {
            int idx = i;
            var slot = Instantiate(inventorySlotPrefab, contentParent);

            var label = slot.GetComponentInChildren<Text>();
            if (label) label.text = inventoryItems[i].name;

            var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            var sr = inventoryItems[i].GetComponent<SpriteRenderer>();
            if (icon && sr) icon.sprite = sr.sprite;

            var button = slot.GetComponent<Button>();
            button.onClick.AddListener(() => SpawnAndGrab(idx));

            // Stop the ScrollRect from swallowing pointer events on the slot.
            // Without this, a slow click gets mistaken for a scroll drag and
            // the button press never registers — causing the "mash to spawn" issue.
            var trigger = slot.GetComponent<EventTrigger>() ?? slot.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((_) => scrollRect.StopMovement());
            trigger.triggers.Add(pointerDown);
        }
    }

    void SpawnAndGrab(int index)
    {
        GameObject prefab = inventoryItems[index];
        Selectable sel = prefab.GetComponent<Selectable>();

        if (sel == null)
        {
            Debug.LogWarning($"[InventoryPanel] '{prefab.name}' has no Selectable component.");
            return;
        }

        // Spawn at the centre of the bounding box, but push Y up to at least
        // the bottom of the bounds so it never starts below the floor.
        Vector3 boundsCenter = (sel.BoundsMin + sel.BoundsMax) / 2f;
        float safeY = Mathf.Max(boundsCenter.y, sel.BoundsMin.y + 0.5f);
        Vector3 spawnPos = new Vector3(boundsCenter.x, safeY, boundsCenter.z);

        GameObject spawned = Instantiate(prefab, spawnPos, Quaternion.identity);
        Selectable spawnedSelectable = spawned.GetComponent<Selectable>();

        spawnedSelectable.BeginDragFromInventory(spawnPos);
    }

    // ── Mouse-wheel scroll while hovering panel ────────────────────
    void Update()
    {
        if (!_panelVisible) return;
        if (RectTransformUtility.RectangleContainsScreenPoint(_panelRect, Input.mousePosition))
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                scrollRect.verticalNormalizedPosition + Input.mouseScrollDelta.y * 0.1f);
    }

    public void TogglePanel()
    {
        _panelVisible = !_panelVisible;
        StopAllCoroutines();
        StartCoroutine(SlidePanel(_panelVisible ? _shownX : _hiddenX));
    }

    IEnumerator SlidePanel(float targetX)
    {
        float duration = 0.25f, elapsed = 0f;
        float startX = _panelRect.anchoredPosition.x;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            _panelRect.anchoredPosition = new Vector2(
                Mathf.Lerp(startX, targetX, t), _panelRect.anchoredPosition.y);
            yield return null;
        }
        _panelRect.anchoredPosition = new Vector2(targetX, _panelRect.anchoredPosition.y);
    }
}