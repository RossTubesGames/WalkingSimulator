using UnityEngine;

public class PickupOrientation : MonoBehaviour
{
    [Tooltip("Local rotation to apply when this item is picked up.")]
    public Vector3 rotationOffset;

    [Tooltip("Local position offset relative to the hold point.")]
    public Vector3 positionOffset;
}
