using System.Collections;
using UnityEngine;

public class MagnetToFish : MonoBehaviour
{
    [Header("References")]
    public Transform attachPoint;   // The tip of the fishing rod

    [Header("Settings")]
    public float attachRange = 3f;  // How close the player must be to attach the magnet

    private bool hasMagnet = false;
    private Transform player;
    private PickUpItem pickupSystem;
    private GameObject magnetRef;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        pickupSystem = FindAnyObjectByType<PickUpItem>();
    }

    private void Update()
    {
        if (player == null || pickupSystem == null) return;

        // Attach attempt
        if (!hasMagnet && Input.GetKeyDown(KeyCode.F))
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance <= attachRange)
                TryAttachMagnet();
        }

        // Keep in sync and enforce parenting if something else changes it
        if (hasMagnet && magnetRef != null)
        {
            if (magnetRef.transform.parent != attachPoint)
            {
                // safety: re-parent if some other script unparented it
                magnetRef.transform.SetParent(attachPoint);
                magnetRef.transform.localPosition = Vector3.zero;
                magnetRef.transform.localRotation = Quaternion.identity;
            }

            // explicit follow (in case parenting is momentarily overridden)
            magnetRef.transform.position = attachPoint.position;
            magnetRef.transform.rotation = attachPoint.rotation;
        }
    }

    private void TryAttachMagnet()
    {
        if (pickupSystem.holdPosition.childCount > 0)
        {
            GameObject heldItem = pickupSystem.holdPosition.GetChild(0).gameObject;

            if (heldItem.GetComponent<MagnetMarker>() != null)
            {
                AttachMagnet(heldItem);
            }
            else
            {
                Debug.Log("You must hold the magnet to attach it to the fishing rod!");
            }
        }
        else
        {
            Debug.Log("You're not holding any item!");
        }
    }

    private void AttachMagnet(GameObject magnet)
    {
        // Disable physics + collisions
        Rigidbody rb = magnet.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider col = magnet.GetComponent<Collider>();
        if (col) col.enabled = false;

        // If marked Static in editor, this can cause headaches; make sure it's not.
        if (magnet.isStatic) magnet.isStatic = false;  // also uncheck Static in the Inspector

        // Place at attach point now
        magnet.transform.position = attachPoint.position;
        magnet.transform.rotation = attachPoint.rotation;

        // Keep a reference for follow logic
        magnetRef = magnet;

        // Re-parent next frame (lets the pickup system finish any state changes)
        StartCoroutine(DelayedParent(magnet));

        hasMagnet = true;
        Debug.Log("Magnet successfully attached to the fishing rod!");
    }

    private IEnumerator DelayedParent(GameObject magnet)
    {
        yield return null; // wait one frame
        magnet.transform.SetParent(attachPoint);
        magnet.transform.localPosition = Vector3.zero;
        magnet.transform.localRotation = Quaternion.identity;
        Debug.Log("Magnet parented under attach point after delay!");
    }

    public bool HasMagnetAttached()
    {
        return hasMagnet;
    }
}
