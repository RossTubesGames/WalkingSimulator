using UnityEngine;

public class PickupGrip : MonoBehaviour
/// Put this on items that need a precise hand placement (e.g., fishing rod).
/// Assign a child transform named however you like (e.g., "GripPoint") that
/// marks where the player's hand should be on the item.
{
    [Tooltip("Child transform on this item that should align to the player's holdPosition.")]
    public Transform grip;

    [Header("Optional fine-tuning (applied after alignment)")]
    public Vector3 extraLocalPosition;   // small nudges, in holdPosition space
    public Vector3 extraLocalEuler;      // small rotations, in holdPosition space

    private void OnDrawGizmosSelected()
    {
        if (!grip) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(grip.position, 0.03f);
        Gizmos.DrawRay(grip.position, grip.right * 0.12f);  // X
        Gizmos.color = Color.green;
        Gizmos.DrawRay(grip.position, grip.up * 0.12f);     // Y
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(grip.position, grip.forward * 0.12f); // Z
    }
}
