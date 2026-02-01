using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorInteraction : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("Target Y rotation (relative to the current rotation) when the door opens.")]
    public float openRotationOffsetY = -110f;   // e.g., -110 for right door, +60 for left door
    public float openSpeed = 2f;
    public float interactDistance = 3.5f;

    [Header("Requirements")]
    public AudioClip openSound;                 // Optional: sound when door opens

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion targetRotation;

    private Transform playerCamera;
    private PickUpItem pickupSystem;
    private AudioSource audioSource;

    private void Start()
    {
        closedRotation = transform.rotation;
        targetRotation = closedRotation;

        // Ensure we have the player's camera
        if (Camera.main != null)
            playerCamera = Camera.main.transform;
        else
            Debug.LogWarning($"[{name}] No MainCamera found. Tag your camera as 'MainCamera'.");

        // Find pickup system in scene
        pickupSystem = FindAnyObjectByType<PickUpItem>();
        if (pickupSystem == null)
            Debug.LogWarning($"[{name}] No PickUpItem script found in scene!");

        // Ensure AudioSource exists if we need it
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && openSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Make sure door has a collider and it's not a trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError($"[{name}] No collider found! Add a MeshCollider or BoxCollider.");
        else if (col.isTrigger)
            Debug.LogWarning($"[{name}] Collider is set as Trigger. Consider disabling IsTrigger for solid raycast detection.");
    }

    private void Update()
    {
        // Smoothly rotate door toward target
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * openSpeed);

        // Player interaction
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        Debug.DrawRay(playerCamera.position, playerCamera.forward * interactDistance, Color.yellow, 0.2f);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            // This is the key fix — find the DoorInteraction in the parent hierarchy
            DoorInteraction hitDoor = hit.collider.GetComponentInParent<DoorInteraction>();

            if (hitDoor == this && !isOpen)
            {
                // Check if the player is holding something
                if (pickupSystem != null && pickupSystem.holdPosition != null && pickupSystem.holdPosition.childCount > 0)
                {
                    GameObject heldItem = pickupSystem.holdPosition.GetChild(0).gameObject;
                    KeyItem key = heldItem.GetComponent<KeyItem>();

                    if (key != null)
                    {
                        OpenDoor();
                    }
                    else
                    {
                        Debug.Log("You must hold a key item to open this door.");
                    }
                }
                else
                {
                    Debug.Log("You are not holding any item.");
                }
            }
        }
    }

    private void OpenDoor()
    {
        isOpen = true;

        targetRotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y + openRotationOffsetY,
            transform.eulerAngles.z
        );

        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);

        Debug.Log($"{gameObject.name} is opening...");
    }
}
