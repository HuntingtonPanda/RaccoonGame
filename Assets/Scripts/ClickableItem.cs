using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public class ClickableItem : MonoBehaviour
{
    // Optional: keep your existing PickupItem for kind + bobbing/visuals.
    // This script only handles being clicked and disappearing.

    bool collected;

    void OnEnable() { collected = false; }

    void Collect()
    {
        if (collected) return;
        collected = true;
        MiniGameManager.Instance?.OnItemCollected(this);
        Destroy(gameObject);
    }

    void Update()
    {
        // Click to collect (supports New Input System and legacy)
        #if ENABLE_INPUT_SYSTEM
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            var cam = Camera.main;
            if (!cam) return;
            Vector3 sp = mouse.position.ReadValue();
            Vector3 wp3 = cam.ScreenToWorldPoint(sp);
            Vector2 wp = new Vector2(wp3.x, wp3.y);

            // Only collect if THIS object was clicked
            var hit = Physics2D.OverlapPoint(wp);
            if (hit && hit.transform == transform) Collect();
        }
        #else
        if (Input.GetMouseButtonDown(0))
        {
            var cam = Camera.main;
            if (!cam) return;
            Vector3 wp3 = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 wp = new Vector2(wp3.x, wp3.y);

            var hit = Physics2D.OverlapPoint(wp);
            if (hit && hit.transform == transform) Collect();
        }
        #endif
    }
}
