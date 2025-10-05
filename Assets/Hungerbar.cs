using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HungerBarSimple : MonoBehaviour
{
    [Header("Prefabs (UI)")]
    [Tooltip("UI prefab for a FULL segment (e.g., square). Must have RectTransform + Image.")]
    public GameObject fullPrefab;
    [Tooltip("UI prefab for an EMPTY segment (e.g., triangle). Must have RectTransform + Image.")]
    public GameObject emptyPrefab;

    [Header("Bar Settings")]
    [SerializeField] private int NUM_BARS = 5;      // how many segments total (treat as your 'constant')
    [SerializeField] private float segmentSize = 28f;
    [SerializeField] private float spacing = 6f;
    [SerializeField] private Vector2 margin = new Vector2(16, 16); // from bottom-left

    [Header("Timer")]
    [SerializeField] private float totalDuration = 30f; // seconds for ALL bars to go full->empty
    [SerializeField] private bool loop = true;

    private readonly List<(Image fullImg, Image emptyImg)> segments = new();
    private float elapsed;

    // --- Setup ---
    private void Awake()
    {
        var canvas = EnsureCanvas();
        var root = new GameObject("HungerBarRoot", typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);

        var rt = (RectTransform)root.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 0);
        rt.anchoredPosition = margin;

        // Layout
        var layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.LowerLeft;
        layout.childControlHeight = layout.childControlWidth = false;
        layout.childForceExpandHeight = layout.childForceExpandWidth = false;

        for (int i = 0; i < NUM_BARS; i++)
        {
            // A container per segment so both full/empty can overlap & stretch
            var holder = new GameObject($"Bar_{i}", typeof(RectTransform));
            holder.transform.SetParent(root.transform, false);
            var hrt = (RectTransform)holder.transform;
            hrt.sizeDelta = new Vector2(segmentSize, segmentSize);

            // FULL
            var full = Instantiate(fullPrefab, holder.transform).GetComponent<Image>();
            Stretch(full.rectTransform);

            // EMPTY
            var empty = Instantiate(emptyPrefab, holder.transform).GetComponent<Image>();
            Stretch(empty.rectTransform);

            // Start fully "full"
            SetAlpha(full, 1f);
            SetAlpha(empty, 0f);

            segments.Add((full, empty));
        }

        Debug.Log($"[HungerBar] Built {NUM_BARS} bars under {canvas.name}");
    }

    private void Update()
    {
        if (totalDuration <= 0f || NUM_BARS <= 0) return;

        elapsed += Time.deltaTime;
        if (elapsed > totalDuration)
        {
            if (loop) elapsed %= totalDuration;
            else elapsed = totalDuration;
        }

        float perSeg = totalDuration / NUM_BARS;

        for (int i = 0; i < NUM_BARS; i++)
        {
            int rev = NUM_BARS - 1 - i;

            float start = i * perSeg;
            float end = (i + 1) * perSeg;
            float t = Mathf.InverseLerp(start, end, elapsed); // 0->1 during this segment’s turn

            // Crossfade: full fades OUT, empty fades IN
            SetAlpha(segments[rev].fullImg, 1f - t);
            SetAlpha(segments[rev].emptyImg, t);
        }
    }

    /// <summary>
    /// Optional: drive it by hunger (1 = full, 0 = empty) instead of time.
    /// </summary>
    public void SetHunger01(float hunger01)
    {
        hunger01 = Mathf.Clamp01(hunger01);

        // Example mapping: hunger=1 means all full; hunger=0 means all empty
        // Convert to how many bars should be empty (from left to right)
        float emptyBarsExact = (1f - hunger01) * NUM_BARS;

        for (int i = 0; i < NUM_BARS; i++)
        {
            // t=0 => full, t=1 => empty; allow partial fade on the current bar
            float t = Mathf.Clamp01(emptyBarsExact - i);
            SetAlpha(segments[i].fullImg, 1f - t);
            SetAlpha(segments[i].emptyImg, t);
        }
    }

    // --- Helpers ---
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
        // Use an existing Overlay canvas if present
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
