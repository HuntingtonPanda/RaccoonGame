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
        if (lockedForever) return;

        if (miniGameRoot) miniGameRoot.SetActive(true);
        SetPlayingUI(true);

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

    public void OnGameEnded()
    {
        if (lockedForever) { ShowLockedUIIfNeeded(); return; }
        SetPlayingUI(false);
    }

    // -------- End views (Close-only, no replay) --------
    public void ShowWin(int collected, int total)
    {
        LockForever();
        FinalizeEndUI();
    }

    public void ShowGameOver(int collected, int total)
    {
        LockForever();
        FinalizeEndUI();
    }

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
    }
}
