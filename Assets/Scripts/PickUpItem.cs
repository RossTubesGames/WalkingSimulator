using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [Header("Player Hold")]
    public Transform holdPosition;

    [Header("Pickup Detection")]
    public float pickupRange = 3f;
    public float pickupAngle = 30f;

    [Header("Story Trigger")]
    [SerializeField] private GameObject paddle;
    [SerializeField] private GameObject FishingRod;
    [SerializeField] private GameObject Key;
    [SerializeField] private GameObject Hammer;

    [SerializeField] private Dialoge dialogue; // drag your Dialogue object here (or auto-find)

    [SerializeField] private string paddleDialogueId = "PaddlePickedUp";
    [SerializeField] private string fishingRodDialogueId = "FishingRodPickedUp";
    [SerializeField] private string keyDialogueId = "KeyPickedUp";
    [SerializeField] private string hammerDialogueId = "HammerPickedUp";

    [SerializeField] private bool triggerPaddleDialogueOnce = true;
    [SerializeField] private bool triggerFishingDialogueOnce = true;
    [SerializeField] private bool triggerKeyDialogueOnce = true;
    [SerializeField] private bool triggerHammerDialogueOnce = true;

    // runtime
    private GameObject heldItem;
    private Rigidbody heldRb;

    // cached rigidbody settings
    private bool prevUseGravity;
    private bool prevIsKinematic;
    private bool prevDetectCollisions;
    private RigidbodyConstraints prevConstraints;

    private bool paddleDialogueTriggered;
    private bool fishingDialogueTriggered;
    private bool keyDialogueTriggered;
    private bool hammerDialogueTriggered;

    private void Awake()
    {
        if (dialogue == null)
            dialogue = FindObjectOfType<Dialoge>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldItem == null)
                TryPickup();
            else
                DropItem();
        }
    }

    void TryPickup()
    {
        Collider[] hits = Physics.OverlapSphere(Camera.main.transform.position, pickupRange);
        GameObject bestCandidate = null;
        float bestDot = Mathf.Cos(pickupAngle * Mathf.Deg2Rad);

        foreach (Collider col in hits)
        {
            if (!col.CompareTag("Pickup")) continue;

            Vector3 toTarget = (col.transform.position - Camera.main.transform.position).normalized;
            float dot = Vector3.Dot(Camera.main.transform.forward, toTarget);

            if (dot > bestDot)
            {
                bestDot = dot;
                bestCandidate = col.gameObject;
            }
        }

        if (bestCandidate == null) return;

        heldItem = bestCandidate;

        // Trigger dialogue if this pickup is the paddle
        if (paddle != null && heldItem == paddle)
        {
            if (!triggerPaddleDialogueOnce || !paddleDialogueTriggered)
            {
                paddleDialogueTriggered = true;

                if (dialogue != null) dialogue.PlayDialogue(paddleDialogueId);
                else Debug.LogWarning("PickUpItem: Dialogue reference is missing.");
            }
        }

        // Trigger dialogue if this pickup is the fishing rod
        if (FishingRod != null && heldItem == FishingRod)
        {
            if (!triggerFishingDialogueOnce || !fishingDialogueTriggered)
            {
                fishingDialogueTriggered = true;

                if (dialogue != null) dialogue.PlayDialogue(fishingRodDialogueId);
                else Debug.LogWarning("PickUpItem: Dialogue reference is missing.");
            }
        }

        // Trigger dialogue if this pickup is the key
        if (Key != null && heldItem == Key)
        {
            if (!triggerKeyDialogueOnce || !keyDialogueTriggered)
            {
                keyDialogueTriggered = true;

                if (dialogue != null) dialogue.PlayDialogue(keyDialogueId);
                else Debug.LogWarning("PickUpItem: Dialogue reference is missing.");
            }
        }

        // Trigger dialogue if this pickup is the hammer
        if (Hammer != null && heldItem == Hammer)
        {
            if (!triggerHammerDialogueOnce || !hammerDialogueTriggered)
            {
                hammerDialogueTriggered = true;

                if (dialogue != null) dialogue.PlayDialogue(hammerDialogueId);
                else Debug.LogWarning("PickUpItem: Dialogue reference is missing.");
            }
        }

        // --- Physics setup ---
        if (heldItem.TryGetComponent(out heldRb))
        {
            prevUseGravity = heldRb.useGravity;
            prevIsKinematic = heldRb.isKinematic;
            prevDetectCollisions = heldRb.detectCollisions;
            prevConstraints = heldRb.constraints;

            heldRb.useGravity = false;
            heldRb.isKinematic = true;
            heldRb.detectCollisions = false;
            heldRb.constraints = RigidbodyConstraints.FreezeAll;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
        }

        // --- ALIGNMENT LOGIC ---
        if (heldItem.TryGetComponent(out PickupGrip grip) && grip.grip != null)
        {
            Quaternion targetHoldRot = holdPosition.rotation * Quaternion.Euler(grip.extraLocalEuler);
            Quaternion itemWorldRot = targetHoldRot * Quaternion.Inverse(grip.grip.localRotation);

            Vector3 itemWorldPos =
                holdPosition.position +
                (targetHoldRot * grip.extraLocalPosition) -
                (itemWorldRot * grip.grip.localPosition);

            heldItem.transform.SetPositionAndRotation(itemWorldPos, itemWorldRot);
            heldItem.transform.SetParent(holdPosition, true);
        }
        else if (heldItem.TryGetComponent(out PickupOrientation orientation))
        {
            heldItem.transform.SetParent(holdPosition);
            heldItem.transform.localPosition = orientation.positionOffset;
            heldItem.transform.localRotation = Quaternion.Euler(orientation.rotationOffset);
        }
        else
        {
            heldItem.transform.SetParent(holdPosition);
            heldItem.transform.localPosition = Vector3.zero;
            heldItem.transform.localRotation = Quaternion.identity;
        }
    }

    void DropItem()
    {
        if (heldItem == null) return;

        if (heldRb)
        {
            heldRb.constraints = prevConstraints;
            heldRb.detectCollisions = true;
            heldRb.useGravity = true;
            heldRb.isKinematic = false;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
        }

        heldItem.transform.SetParent(null);
        heldItem = null;
        heldRb = null;
    }

    private void OnDisable()
    {
        if (heldItem != null)
            DropItem();
    }
}
