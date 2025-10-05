using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    [Header("UI (TMP under Window)")]
    public TMP_Text titleText;            // shows status / final score
    public TMP_Text scoreText;            // "Collected: x / N"
    public TMP_Text timerText;

    [Header("Prefabs (what to spawn)")]
    public GameObject foodPrefab;
    public GameObject badPrefab;
    public GameObject collectiblePrefab;

    [Header("Spawn Settings")]
    public int initialItemCount = 12;
    public BoxCollider2D spawnBounds;
    public float minSeparation = 0.6f;
    public int maxPlacementTries = 40;

    [Header("Type Weights")]
    public float foodWeight = 6f;
    public float badWeight = 3f;
    public float collectibleWeight = 1f;

    [Header("Round Rules")]
    public float timeLimit = 30f;

    [Header("Target Highlight")]
    public Color targetColor = new Color(1f, 0.92f, 0.3f, 1f);
    public Color normalColor = Color.white;
    public float targetScale = 1.15f;
    public int   targetSortingOrder = 50;
    public int   normalSortingOrder = 0;

    // state
    int collected;
    float timeLeft;
    bool running;
    int targetCount;

    readonly List<ClickableItem> items = new();
    ClickableItem currentTarget;

    void Awake() => Instance = this;

    void OnEnable()
    {
        running = false;
        collected = 0;
        targetCount = 0;
        UpdateUI(0, timeLimit, 0);
        if (titleText) titleText.text = "Trash Collector";
    }

    public void StartRound()
    {
        // wipe leftovers if any
        CleanupAllItems();

        collected = 0;
        targetCount = Mathf.Max(0, initialItemCount);
        timeLeft = timeLimit;
        running = true;

        SpawnBatch(targetCount);
        if (items.Count > 0) SetTarget(items[0]);

        UpdateUI(collected, timeLeft, targetCount);
    }

    void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
            End(false);
        }

        UpdateUI(collected, timeLeft, targetCount);
    }

    public void OnItemCollected(ClickableItem clicked)
    {
        if (!running || clicked == null) return;
        if (clicked != currentTarget) return; // only the highlighted one counts

        collected++;
        items.Remove(clicked); // ClickableItem destroys itself

        if (collected >= targetCount || items.Count == 0)
        {
            running = false;
            End(true);
        }
        else
        {
            SetTarget(items[0]);
        }

        UpdateUI(collected, timeLeft, targetCount);
    }

    void End(bool win)
    {
        // remove anything left
        CleanupAllItems();

        // set title to include final score
        if (titleText)
        {
            if (win) titleText.text = $"You Win — Collected: {collected}/{targetCount}";
            else     titleText.text = $"Game Over — Collected: {collected}/{targetCount}";
        }

        // lock popup so Start is gone forever; show only Close
        var popup = FindObjectOfType<MiniGamePopup>();
        if (popup)
        {
            if (win) popup.ShowWin(collected, targetCount);
            else     popup.ShowGameOver(collected, targetCount);
        }
    }

    // ------- utilities -------
    void CleanupAllItems()
    {
        foreach (var it in items)
            if (it) Destroy(it.gameObject);
        items.Clear();
        currentTarget = null;
    }

    void SpawnBatch(int count)
    {
        if (!spawnBounds)
        {
            Debug.LogError("[MiniGame] Assign a BoxCollider2D to 'spawnBounds'.");
            return;
        }

        var b = spawnBounds.bounds;
        var placed = new List<Vector2>();

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = ChoosePrefab();
            Vector2 pos;
            int tries = 0;

            do
            {
                pos = new Vector2(
                    Random.Range(b.min.x, b.max.x),
                    Random.Range(b.min.y, b.max.y)
                );
                tries++;
            }
            while (tries < maxPlacementTries && TooClose(pos, placed, minSeparation));

            placed.Add(pos);

            var go = Instantiate(prefab, pos, Quaternion.identity);

            var ci = go.GetComponent<ClickableItem>();
            if (!ci) ci = go.AddComponent<ClickableItem>();

            if (!go.TryGetComponent<Collider2D>(out _))
            {
                var sr = go.GetComponent<SpriteRenderer>();
                var box = go.AddComponent<BoxCollider2D>();
                if (sr && sr.sprite) box.size = sr.sprite.bounds.size;
                box.isTrigger = true;
            }

            items.Add(ci);
            SetNonTargetVisuals(ci);
        }
    }

    bool TooClose(Vector2 p, List<Vector2> list, float minDist)
    {
        float sq = minDist * minDist;
        foreach (var q in list)
            if ((p - q).sqrMagnitude < sq) return true;
        return false;
    }

    GameObject ChoosePrefab()
    {
        float total = foodWeight + badWeight + collectibleWeight;
        float r = Random.value * total;
        if (r < foodWeight) return foodPrefab;
        r -= foodWeight;
        if (r < badWeight) return badPrefab;
        return collectiblePrefab;
    }

    void UpdateUI(int collectedCount, float time, int total)
    {
        if (scoreText) scoreText.text = $"Collected: {collectedCount}/{total}";
        if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(time)}";
    }

    // ------- target visuals -------
    void SetTarget(ClickableItem newTarget)
    {
        if (currentTarget) SetNonTargetVisuals(currentTarget);
        currentTarget = newTarget;

        foreach (var it in items)
        {
            if (!it) continue;
            var col = it.GetComponent<Collider2D>();
            if (col) col.enabled = (it == currentTarget);

            if (it == currentTarget) SetTargetVisuals(it);
            else SetNonTargetVisuals(it);
        }
    }

    void SetTargetVisuals(ClickableItem it)
    {
        var sr = it.GetComponent<SpriteRenderer>();
        if (sr)
        {
            sr.color = targetColor;
            sr.sortingOrder = targetSortingOrder;
        }
        it.transform.localScale = Vector3.one * targetScale;
    }

    void SetNonTargetVisuals(ClickableItem it)
    {
        var sr = it.GetComponent<SpriteRenderer>();
        if (sr)
        {
            sr.color = normalColor;
            sr.sortingOrder = normalSortingOrder;
        }
        it.transform.localScale = Vector3.one;
    }
}
