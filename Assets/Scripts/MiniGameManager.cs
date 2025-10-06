using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MiniGameManager : MonoBehaviour
{
    public static MiniGameManager Instance { get; private set; }

    [Header("UI (TMP under Window)")]
    public TMP_Text titleText;
    public TMP_Text scoreText;
    public TMP_Text timerText;

    [Header("Prefab Lists")]
    public List<GameObject> foodPrefabs = new();
    public List<GameObject> trashPrefabs = new();
    public List<GameObject> collectiblePrefabs = new();

    [Header("Spawn Area")]
    public BoxCollider2D spawnBounds;
    public float minSeparation = 0.6f;
    public int maxPlacementTries = 40;

    [Header("Spawn Weights (low chance collectibles)")]
    public float foodWeight = 6f;
    public float trashWeight = 3f;
    public float collectibleWeight = 0.6f; // low, but we guarantee ≥1

    [Header("Round Rules")]
    public int initialItemCount = 25;
    public float timeLimit = 15f;

    [Header("Target Highlight")]
    public Color targetColor = new Color(1f, 0.92f, 0.3f, 1f);
    public Color normalColor = Color.white;
    public float targetScale = 1.15f;
    public int   targetSortingOrder = 50;
    public int   normalSortingOrder = 0;

    // ---- runtime state ----
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
        CleanupAllItems();

        collected = 0;
        targetCount = Mathf.Max(1, initialItemCount);
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

    // Called by ClickableItem.OnMouseDown() on the CURRENT target only
    public void OnItemCollected(ClickableItem clicked)
    {
        if (!running || clicked == null) return;
        if (clicked != currentTarget) return; // only highlighted one counts

        collected++;
        items.Remove(clicked); // (ClickableItem destroys its own GO)

        if (collected >= targetCount || items.Count == 0)
        {
            running = false;
            End(true);
        }
        else
        {
            SetTarget(items[0]); // next in list (or randomize if you like)
        }

        UpdateUI(collected, timeLeft, targetCount);
    }

    void End(bool win)
    {
        CleanupAllItems();

        if (titleText)
        {
            titleText.text = win
                ? $"You Win — Collected: {collected}/{targetCount}"
                : $"Game Over — Collected: {collected}/{targetCount}";
        }

        var popup = FindObjectOfType<MiniGamePopup>();
        if (popup)
        {
            if (win) popup.ShowWin(collected, targetCount);
            else     popup.ShowGameOver(collected, targetCount);
        }
    }

    // ----------------- Spawning -----------------

    enum Kind { Food, Trash, Collectible }

    void SpawnBatch(int count)
    {
        if (!spawnBounds)
        {
            Debug.LogError("[MiniGame] Assign a BoxCollider2D to 'spawnBounds'.");
            return;
        }

        var b = spawnBounds.bounds;
        var placed = new List<Vector2>();

        // Plan kinds with guarantee: ≥1 collectible
        var plannedKinds = new Kind[count];
        int guaranteeIndex = Random.Range(0, count);
        for (int i = 0; i < count; i++)
            plannedKinds[i] = (i == guaranteeIndex) ? Kind.Collectible : WeightedKind();

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = PickPrefab(plannedKinds[i]);
            if (!prefab)
            {
                prefab = PickAnyAvailable();
                if (!prefab) { Debug.LogError("[MiniGame] All prefab lists are empty."); return; }
            }

            Vector2 pos;
            int tries = 0;
            do
            {
                pos = new Vector2(Random.Range(b.min.x, b.max.x),
                                  Random.Range(b.min.y, b.max.y));
                tries++;
            }
            while (tries < maxPlacementTries && TooClose(pos, placed, minSeparation));

            placed.Add(pos);

            var go = Instantiate(prefab, pos, Quaternion.identity);

            var ci = go.GetComponent<ClickableItem>();
            if (!ci) ci = go.AddComponent<ClickableItem>();

            // add a collider if missing (size to child SR if needed)
            if (!go.TryGetComponent<Collider2D>(out _))
            {
                var anySR = go.GetComponentInChildren<SpriteRenderer>(true);
                var box = go.AddComponent<BoxCollider2D>();
                box.isTrigger = true;
                if (anySR && anySR.sprite)
                {
                    var ws = anySR.bounds;
                    box.size   = ws.size;
                    box.offset = go.transform.InverseTransformPoint(ws.center);
                }
            }

            items.Add(ci);
            SetNonTargetVisuals(ci);
        }
    }

    Kind WeightedKind()
    {
        float total = foodWeight + trashWeight + collectibleWeight;
        if (total <= 0f) return Kind.Food;

        float r = Random.value * total;
        if (r < foodWeight) return Kind.Food;
        r -= foodWeight;
        if (r < trashWeight) return Kind.Trash;
        return Kind.Collectible;
    }

    GameObject PickPrefab(Kind k)
    {
        switch (k)
        {
            case Kind.Food:        return PickFrom(foodPrefabs);
            case Kind.Trash:       return PickFrom(trashPrefabs);
            case Kind.Collectible: return PickFrom(collectiblePrefabs);
        }
        return null;
    }

    GameObject PickAnyAvailable()
    {
        var all = new List<GameObject>();
        all.AddRange(foodPrefabs);
        all.AddRange(trashPrefabs);
        all.AddRange(collectiblePrefabs);
        return PickFrom(all);
    }

    static GameObject PickFrom(List<GameObject> list)
    {
        if (list == null || list.Count == 0) return null;
        return list[Random.Range(0, list.Count)];
    }

    bool TooClose(Vector2 p, List<Vector2> list, float minDist)
    {
        float sq = minDist * minDist;
        foreach (var q in list)
            if ((p - q).sqrMagnitude < sq) return true;
        return false;
    }

    // ----------------- UI / visuals -----------------

    void UpdateUI(int collectedCount, float time, int total)
    {
        if (scoreText) scoreText.text = $"Collected: {collectedCount}/{total}";
        if (timerText) timerText.text = $"Time: {Mathf.CeilToInt(time)}";
    }

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
        var sr = it.GetComponentInChildren<SpriteRenderer>(true);
        if (sr)
        {
            sr.color = targetColor;
            sr.sortingOrder = targetSortingOrder;
        }
        it.transform.localScale = Vector3.one * targetScale;
    }

    void SetNonTargetVisuals(ClickableItem it)
    {
        var sr = it.GetComponentInChildren<SpriteRenderer>(true);
        if (sr)
        {
            sr.color = normalColor;
            sr.sortingOrder = normalSortingOrder;
        }
        it.transform.localScale = Vector3.one;
    }

    void CleanupAllItems()
    {
        foreach (var it in items)
            if (it) Destroy(it.gameObject);
        items.Clear();
        currentTarget = null;
    }
}
