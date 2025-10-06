using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGamePopup : MonoBehaviour
{
    [Header("Popup Roots")]
    public GameObject popupRoot;      // Canvas/MiniGamePopup (gray dimmer)
    public GameObject windowRoot;     // Canvas/MiniGamePopup/Window

    [Header("MiniGame")]
    public GameObject miniGameRoot;   // MiniGameRoot (BG + items + manager)

    [Header("Buttons")]
    public Button startButton;
    public Button closeButton;

    [Header("Optional UI Images for styling")]
    public Image popupBackdropImage;  // Image on MiniGamePopup
    public Image windowBackground;    // Image on Window

    [Header("Summary UI (icon : count)")]
    public RectTransform summaryListRoot;   // VerticalLayout container under Window
    public GameObject summaryRowPrefab;     // Prefab with Image (icon) + TMP_Text (count)

    bool isOpen;
    bool lockedForever;

    void Awake()
    {
        if (popupRoot)  popupRoot.SetActive(false);
        if (windowRoot) windowRoot.SetActive(false);
        if (miniGameRoot) miniGameRoot.SetActive(false);

        if (startButton) startButton.onClick.AddListener(StartMiniGame);
        if (closeButton) closeButton.onClick.AddListener(ClosePopup);

        SetPlayingUI(false);
        ShowLockedUIIfNeeded();
        ClearSummary();
    }

    public void OpenPopup()
    {
        if (isOpen) return;
        isOpen = true;

        if (popupRoot)  popupRoot.SetActive(true);
        if (windowRoot) windowRoot.SetActive(true);
        if (miniGameRoot) miniGameRoot.SetActive(!lockedForever);

        SetPlayingUI(false);
        ShowLockedUIIfNeeded();
        ClearSummary();
    }

    void StartMiniGame()
    {
        if (lockedForever) return;

        if (miniGameRoot) miniGameRoot.SetActive(true);
        SetPlayingUI(true);
        ClearSummary();

        var mgr = FindObjectOfType<MiniGameManager>();
        if (mgr) mgr.StartRound();
        else Debug.LogError("[MiniGame] MiniGameManager not found.");
    }

    public void ClosePopup()
    {
        if (miniGameRoot) miniGameRoot.SetActive(false);
        if (windowRoot) windowRoot.SetActive(false);
        if (popupRoot)  popupRoot.SetActive(false);
        isOpen = false;
        SetPlayingUI(false);
        ClearSummary();
    }

    public void OnGameEnded()
    {
        if (lockedForever) { ShowLockedUIIfNeeded(); return; }
        SetPlayingUI(false);
    }

    // ---------- End views (Close-only, with summary) ----------
    public void ShowWinWithSummary(Dictionary<Sprite,int> summary, int collected, int total)
    {
        LockForever();
        FinalizeEndUI();
        RenderSummary(summary);
    }

    public void ShowGameOverWithSummary(Dictionary<Sprite,int> summary, int collected, int total)
    {
        LockForever();
        FinalizeEndUI();
        RenderSummary(summary);
    }

    // (keep these if other code still calls them)
    public void ShowWin(int collected, int total)        { LockForever(); FinalizeEndUI(); }
    public void ShowGameOver(int collected, int total)   { LockForever(); FinalizeEndUI(); }

    void FinalizeEndUI()
    {
        if (popupRoot)  popupRoot.SetActive(true);
        if (windowRoot) windowRoot.SetActive(true);
        if (miniGameRoot) miniGameRoot.SetActive(false);

        if (startButton) startButton.gameObject.SetActive(false);
        if (closeButton) closeButton.gameObject.SetActive(true);

        if (popupBackdropImage) popupBackdropImage.enabled = true;
        if (windowBackground)
        {
            var c = windowBackground.color; c.a = 0.30f; windowBackground.color = c;
        }
    }

    void LockForever()
    {
        lockedForever = true;
        ShowLockedUIIfNeeded();
    }

    void ShowLockedUIIfNeeded()
    {
        if (!lockedForever) return;
        if (startButton) startButton.gameObject.SetActive(false);
        if (closeButton) closeButton.gameObject.SetActive(true);
        if (popupBackdropImage) popupBackdropImage.enabled = true;
        if (windowBackground)
        {
            var c = windowBackground.color; c.a = 0.30f; windowBackground.color = c;
        }
    }

    void SetPlayingUI(bool playing)
    {
        if (!lockedForever && startButton) startButton.gameObject.SetActive(!playing);
        if (closeButton) closeButton.gameObject.SetActive(!playing);

        if (popupBackdropImage) popupBackdropImage.enabled = !playing;
        if (windowBackground)
        {
            var c = windowBackground.color;
            c.a = playing ? 0f : 0.30f;
            windowBackground.color = c;
        }

        if (playing) ClearSummary();
    }

    // ---------- Summary rendering ----------
    void RenderSummary(Dictionary<Sprite,int> summary)
    {
        if (!summaryListRoot || !summaryRowPrefab)
        {
            Debug.LogWarning("[MiniGamePopup] Assign summaryListRoot & summaryRowPrefab to show the summary.");
            return;
        }

        ClearSummary();
        summaryListRoot.gameObject.SetActive(true);

        if (summary == null || summary.Count == 0)
        {
            var row = Instantiate(summaryRowPrefab, summaryListRoot);
            var img = row.GetComponentInChildren<Image>(true);
            var txt = row.GetComponentInChildren<TMP_Text>(true);
            if (img) img.enabled = false;
            if (txt) txt.text = "No items recorded";
            return;
        }

        var ordered = new List<KeyValuePair<Sprite,int>>(summary);
        ordered.Sort((a,b) => b.Value.CompareTo(a.Value));

        foreach (var kv in ordered)
        {
            var row = Instantiate(summaryRowPrefab, summaryListRoot);
            var img = row.GetComponentInChildren<Image>(true);
            var txt = row.GetComponentInChildren<TMP_Text>(true);
            if (img) { img.sprite = kv.Key; img.preserveAspect = true; img.enabled = true; }
            if (txt) txt.text = $"× {kv.Value}";
        }
    }

    void ClearSummary()
    {
        if (!summaryListRoot) return;
        for (int i = summaryListRoot.childCount - 1; i >= 0; i--)
            Destroy(summaryListRoot.GetChild(i).gameObject);
        summaryListRoot.gameObject.SetActive(false);
    }
}
