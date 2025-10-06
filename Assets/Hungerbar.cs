using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HungerBar : MonoBehaviour
{
    [Header("Prefabs (UI)")]
    [Tooltip("UI prefab for a FULL segment (square). Must be a UI Image (RectTransform + Image).")]
    public GameObject fullPrefab;
    [Tooltip("UI prefab for an EMPTY segment (triangle). Must be a UI Image (RectTransform + Image).")]
    public GameObject emptyPrefab;

    [Header("Bars")]
    [SerializeField] private int NUM_BARS = 5;          // total bars shown
    [SerializeField] private const float SEGMENT_SIZE = 100f;   // px size per bar
    [SerializeField] private const float SPACING = 12f;
    [SerializeField] private Vector2 MARGIN = new Vector2(16, 16); // from bottom-left

    [Header("Timing")]
    [SerializeField] private float timePerBar = 5f;     // seconds to deplete 1 bar
    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool autoResumeAfterRefill = true; // resume drain after refills

    // Fired once when everything is empty
    public event Action OnHungerDepleted;

    private readonly List<(Image fullImg, Image emptyImg)> segments = new();

    // progress measured in "seconds drained"
    private float elapsed = 0f;
    private bool isDraining = false;
    private bool isEmpty = false;

    // cached UI root for rebuilds (I honestly dont know why we need this please dont ask)
    private RectTransform _rootRT;

    // ------------------- Lifecycle -------------------

    private void Awake()
    {
        if (fullPrefab == null || emptyPrefab == null)
        {
            Debug.LogError("[HungerBar] Assign Full & Empty UI Image prefabs (UI Image, not SpriteRenderer).");
            enabled = false;
            return;
        }

        BuildUI();
        UpdateVisuals();

        if (startOnAwake) StartDraining();
    }

    private void Update()
    {
        if (!isDraining || isEmpty || NUM_BARS <= 0) return;

        float totalTime = timePerBar * NUM_BARS;
        elapsed += Time.deltaTime;

        if (elapsed >= totalTime)
        {
            elapsed = totalTime;
            isDraining = false;
            isEmpty = true;
            UpdateVisuals();
            OnHungerDepleted?.Invoke();      // signal end sceen
            return;
        }

        UpdateVisuals();
    }

    // ------------------- UI Build/Rebuild -------------------

    private void BuildUI()
    {
        var canvas = EnsureCanvas();

        // Root holder (again idk)
        var rootGO = new GameObject("HungerBarRoot", typeof(RectTransform));
        rootGO.transform.SetParent(canvas.transform, false);

        _rootRT = (RectTransform)rootGO.transform;
        _rootRT.anchorMin = _rootRT.anchorMax = _rootRT.pivot = new Vector2(0, 0);
        _rootRT.anchoredPosition = MARGIN;

        var layout = rootGO.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = SPACING;
        layout.childAlignment = TextAnchor.LowerLeft;
        layout.childControlHeight = layout.childControlWidth = false;
        layout.childForceExpandHeight = layout.childForceExpandWidth = false;

        segments.Clear();
        for (int i = 0; i < NUM_BARS; i++)
            segments.Add(CreateSegment(_rootRT));
    }

    private (Image fullImg, Image emptyImg) CreateSegment(Transform parent)
    {
        var holder = new GameObject($"Bar_{segments.Count}", typeof(RectTransform));
        holder.transform.SetParent(parent, false);
        ((RectTransform)holder.transform).sizeDelta = new Vector2(SEGMENT_SIZE, SEGMENT_SIZE);

        var full = Instantiate(fullPrefab, holder.transform).GetComponent<Image>();
        Stretch(full.rectTransform);

        var empty = Instantiate(emptyPrefab, holder.transform).GetComponent<Image>();
        Stretch(empty.rectTransform);

        SetAlpha(full, 1f); // start full
        SetAlpha(empty, 0f);

        return (full, empty);
    }

    private void RebuildUI(int oldCount)
    {
        if (_rootRT == null)
        {
            BuildUI();
            UpdateVisuals();
            return;
        }

        // preserve progress ratio based on total duration before/after
        float oldTotal = timePerBar * Mathf.Max(oldCount, 1);
        float progress01 = oldTotal > 0f ? Mathf.Clamp01(elapsed / oldTotal) : 1f;

        // wipe
        for (int i = _rootRT.childCount - 1; i >= 0; i--)
            Destroy(_rootRT.GetChild(i).gameObject);
        segments.Clear();

        // recreate
        for (int i = 0; i < NUM_BARS; i++)
            segments.Add(CreateSegment(_rootRT));

        // restore to same relative drain point
        float newTotal = timePerBar * Mathf.Max(NUM_BARS, 1);
        elapsed = progress01 * newTotal;

        isEmpty = (NUM_BARS == 0) || Mathf.Approximately(elapsed, newTotal);
        UpdateVisuals();

        if (isEmpty)
        {
            isDraining = false;
            OnHungerDepleted?.Invoke();
        }
    }

    // ------------------- Visuals -------------------

    private void UpdateVisuals()
    {
        if (NUM_BARS <= 0 || segments.Count == 0) return;

        float perSeg = timePerBar;

        for (int i = 0; i < NUM_BARS; i++)
        {
            // right -> left (reverse visual index)
            int rev = NUM_BARS - 1 - i;

            float start = i * perSeg;
            float end = (i + 1) * perSeg;
            float t = Mathf.InverseLerp(start, end, elapsed); // 0..1 for this segment

            // full fades OUT, empty fades IN
            SetAlpha(segments[rev].fullImg, 1f - t);
            SetAlpha(segments[rev].emptyImg, t);
        }
    }

    // ------------------- Public API -------------------

    public void StartDraining()
    {
        if (!isEmpty) isDraining = true;
    }

    public void StopDraining()
    {
        isDraining = false;
    }

    public void ResetHunger()
    {
        elapsed = 0f;
        isEmpty = false;
        isDraining = true;
        UpdateVisuals();
    }

    // Refill by seconds (partial bars allowed I think lol)
    public void RefillSeconds(float seconds)
    {
        if (seconds <= 0f || NUM_BARS <= 0) return;

        float totalTime = timePerBar * NUM_BARS;
        elapsed = Mathf.Max(0f, elapsed - seconds);

        if (elapsed < totalTime) isEmpty = false;

        UpdateVisuals();

        if (autoResumeAfterRefill && !isEmpty)
            isDraining = true;
    }

    public void RefillBars(int bars)
    {
        if (bars <= 0) return;
        RefillSeconds(bars * timePerBar);
    }

    public void ConsumeBars(int bars)
    {
        if (bars <= 0 || NUM_BARS <= 0) return;

        float totalTime = timePerBar * NUM_BARS;
        elapsed = Mathf.Min(totalTime, elapsed + bars * timePerBar);
        UpdateVisuals();

        if (elapsed >= totalTime && !isEmpty)
        {
            isDraining = false;
            isEmpty = true;
            OnHungerDepleted?.Invoke();
        }
    }

    // Directly set hunger level (1 = full, 0 = empty)
    public void SetHunger01(float hunger01)
    {
        hunger01 = Mathf.Clamp01(hunger01);
        float totalTime = timePerBar * Mathf.Max(NUM_BARS, 1);
        elapsed = (1f - hunger01) * totalTime;

        isEmpty = elapsed >= totalTime - 1e-4f;
        UpdateVisuals();

        if (autoResumeAfterRefill && !isEmpty)
            isDraining = true;
    }

    // Add N bars to capacity (appears on the right)
    public void AddBar(int count = 1)
    {
        if (count <= 0) return;
        int old = NUM_BARS;
        NUM_BARS += count;

        // If we were empty, adding capacity makes us “not empty” in spirit.
        if (isEmpty) isEmpty = false;

        RebuildUI(old);

        if (autoResumeAfterRefill && !isEmpty)
            isDraining = true;
    }

    // Remove N bars from capacity
    public void RemoveBar(int count = 1)
    {
        if (count <= 0) return;
        int old = NUM_BARS;
        NUM_BARS = Mathf.Max(0, NUM_BARS - count);

        RebuildUI(old);
    }

    // ------------------- Helpers -------------------

    private static void SetAlpha(Image img, float a)
    {
        var c = img.color; c.a = a; img.color = c;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private Canvas EnsureCanvas()
    {
        // Use an existing Overlay canvas if available
        foreach (var c in FindObjectsOfType<Canvas>())
            if (c.isActiveAndEnabled && c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;

        // Otherwise create one
        var go = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 1f;

        if (FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        return canvas;
    }
}