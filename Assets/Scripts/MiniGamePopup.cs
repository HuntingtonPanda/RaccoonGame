using UnityEngine;
using UnityEngine.UI;

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

    bool isOpen;
    bool lockedForever;               // <<< NEW: once true, Start is permanently disabled

    void Awake()
    {
        if (popupRoot)  popupRoot.SetActive(false);
        if (windowRoot) windowRoot.SetActive(false);
        if (miniGameRoot) miniGameRoot.SetActive(false);

        startButton.onClick.AddListener(StartMiniGame);
        closeButton.onClick.AddListener(ClosePopup);

        SetPlayingUI(false);
        ShowLockedUIIfNeeded();
    }

    public void OpenPopup()
    {
        if (isOpen) return;
        isOpen = true;

        if (popupRoot)  popupRoot.SetActive(true);
        if (windowRoot) windowRoot.SetActive(true);

        // Show BG/world only if not locked
        if (miniGameRoot) miniGameRoot.SetActive(!lockedForever);

        SetPlayingUI(false);
        ShowLockedUIIfNeeded();
    }

    void StartMiniGame()
    {
        if (lockedForever) return; // <<< block repeats forever

        if (miniGameRoot) miniGameRoot.SetActive(true);
        SetPlayingUI(true); // hide Start/Close, dimmer off during play

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
    }

    // Called by manager on normal end (if you still use it for any path)
    public void OnGameEnded()
    {
        if (lockedForever) { ShowLockedUIIfNeeded(); return; }
        SetPlayingUI(false); // default: show Start/Close again (unused once locked)
    }

    // ============ NEW: Win / Game Over views (Close-only) ============

    public void ShowWin(int collected, int total)
    {
        LockForever();
        // Manager already set the title text to show the score.
        // Keep popup visible, world off, only Close:
        FinalizeEndUI();
    }

    public void ShowGameOver(int collected, int total)
    {
        LockForever();
        // Manager already set the title text to "Game Over — Collected X/Y".
        FinalizeEndUI();
    }

    void FinalizeEndUI()
    {
        if (popupRoot)  popupRoot.SetActive(true);
        if (windowRoot) windowRoot.SetActive(true);
        if (miniGameRoot) miniGameRoot.SetActive(false);

        // Only Close available
        if (startButton) startButton.gameObject.SetActive(false);
        if (closeButton) closeButton.gameObject.SetActive(true);

        // Re-enable dim/background so text is readable
        if (popupBackdropImage) popupBackdropImage.enabled = true;
        if (windowBackground)
        {
            var c = windowBackground.color;
            c.a = 0.30f;
            windowBackground.color = c;
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

    // Show/hide UI while actively playing
    void SetPlayingUI(bool playing)
    {
        // If we're locked, Start must stay hidden regardless
        if (!lockedForever && startButton) startButton.gameObject.SetActive(!playing);
        if (closeButton) closeButton.gameObject.SetActive(!playing);

        if (popupBackdropImage) popupBackdropImage.enabled = !playing;
        if (windowBackground)
        {
            var c = windowBackground.color;
            c.a = playing ? 0f : 0.30f;
            windowBackground.color = c;
        }
    }
}
