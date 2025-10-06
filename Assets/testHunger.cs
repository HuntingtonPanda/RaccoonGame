using UnityEngine;

public class testHunger : MonoBehaviour
{
    // Drag your HungerBarSimple from the scene here in the Inspector (recommended).
    [SerializeField] private HungerBar hunger;

    private void Awake()
    {
        // If you forgot to assign it, try to find one at runtime (Unity 2022.2+/2023+ API):
        if (hunger == null) hunger = Object.FindFirstObjectByType<HungerBar>();
        if (hunger == null)
            Debug.LogError("testHunger: No HungerBarSimple found in the scene. Assign it in the Inspector.");
    }

    private void Update()
    {
        if (hunger == null) return;

        // z: refill by 1 bar
        if (Input.GetKeyDown(KeyCode.Z))
        {
            hunger.RefillBars(1);
            Debug.Log("Refilled by 1 bar");
        }
        // x: drain by 1 bar
        if (Input.GetKeyDown(KeyCode.X))
        {
            hunger.ConsumeBars(1);
            Debug.Log("Drained by 1 bar");
        }
        // c: pause drain
        if (Input.GetKeyDown(KeyCode.C))
        {
            hunger.StopDraining();
            Debug.Log("Paused draining");
        }
        // v: start/resume drain
        if (Input.GetKeyDown(KeyCode.V))
        {
            hunger.StartDraining();
            Debug.Log("Resumed draining");
        }
        // q: increase bars by 1
        if (Input.GetKeyDown(KeyCode.Q))
        {
            hunger.AddBar(1);
            Debug.Log("Increased bar count by 1");
        }
        // e: decrease bars by 1
        if (Input.GetKeyDown(KeyCode.E))
        {
            hunger.RemoveBar(1);
            Debug.Log("Decreased bar count by 1");
        }
    }
}
