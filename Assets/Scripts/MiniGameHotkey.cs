using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class MiniGameHotkey : MonoBehaviour
{
    public MiniGamePopup popup;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.mKey.wasPressedThisFrame)
            popup.OpenPopup();
    }
}
