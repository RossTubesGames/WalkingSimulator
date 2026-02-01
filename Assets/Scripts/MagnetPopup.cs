using System.Collections;
using UnityEngine;

public class MagnetPopup : MonoBehaviour
{
    [Header("References")]
    public GameObject magnet;
    public Transform popupPoint;
    public Transform player; // assign your Player transform here in the Inspector

    [Header("Activation Settings")]
    public float activationRange = 3f; // distance within which F works

    [Header("Animation Settings")]
    public float jumpHeight = 1.5f;
    public float jumpDuration = 1.0f;
    public AudioClip popSound;

    private bool hasPopped = false;
    private AudioSource audioSrc;
    private Collider magnetCollider;

    void Start()
    {
        if (magnet == null || popupPoint == null)
        {
            Debug.LogWarning("MagnetPopup: Missing magnet or popupPoint reference.");
        }

        magnetCollider = magnet.GetComponent<Collider>();
        if (magnetCollider != null)
            magnetCollider.enabled = false; // hide collider until it pops

        if (popSound != null)
            audioSrc = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Only allow activation if player is close enough
        if (player != null)
        {
            float distance = Vector3.Distance(player.position, transform.position);

            if (distance <= activationRange && Input.GetKeyDown(KeyCode.F) && !hasPopped)
            {
                StartCoroutine(PopMagnet());
            }
        }
    }

    IEnumerator PopMagnet()
    {
        hasPopped = true;

        if (audioSrc && popSound)
            audioSrc.PlayOneShot(popSound);

        Vector3 startPos = magnet.transform.position;
        Vector3 endPos = popupPoint.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            float heightOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            Vector3 flatPos = Vector3.Lerp(startPos, endPos, t);
            magnet.transform.position = flatPos + Vector3.up * heightOffset;
            magnet.transform.Rotate(Vector3.up * 360 * Time.deltaTime, Space.World);
            yield return null;
        }

        magnet.transform.position = endPos;

        if (magnetCollider != null)
            magnetCollider.enabled = true;

        Debug.Log("Magnet is ready to pick up!");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
    }
}
