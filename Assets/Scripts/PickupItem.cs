using UnityEngine;

public enum ItemKind { Food, Bad, Collectible }

[RequireComponent(typeof(SpriteRenderer))]
public class PickupItem : MonoBehaviour
{
    [Header("Type of this item")]
    public ItemKind kind = ItemKind.Food;

    [Header("Optional bobbing (visual only)")]
    public bool bob = true;
    public float bobAmplitude = 0.08f;
    public float bobSpeed = 2.2f;

    private Vector3 basePos;

    void OnEnable() => basePos = transform.position;

    void Update()
    {
        if (!bob) return;
        transform.position = basePos + new Vector3(0f, Mathf.Sin(Time.time * bobSpeed) * bobAmplitude, 0f);
    }
}
