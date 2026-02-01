using UnityEngine;

public class HammerSwordHit : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float smashRange = 10f;
    public LayerMask swordLayer;          // only 'sword' layer checked

    [Header("Sword Logic")]
    public int hitsToFix = 5;

    [Header("Where the fixed sword should go")]
    public Transform swordSpawn;          // assign SwordSpawn in pirate's hand

    [Header("Optional VFX")]
    public ParticleSystem smashParticlePrefab;

    [Header("Dialogue Trigger (On Sword Fixed)")]
    [SerializeField] private Dialoge dialogue;
    [SerializeField] private string swordFixedDialogueId = "SwordFixed";
    [SerializeField] private bool triggerSwordFixedDialogueOnce = true;

    private int currentHits = 0;
    private bool isFixed = false;
    private bool swordFixedDialogueTriggered = false;

    private void Awake()
    {
        if (dialogue == null)
            dialogue = FindObjectOfType<Dialoge>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || isFixed)
            return;

        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // Only hits colliders on the swordLayer
        if (Physics.Raycast(ray, out RaycastHit hit, smashRange, swordLayer))
        {
            Debug.Log("Hit sword-layer object: " + hit.collider.name);

            currentHits++;
            Debug.Log("Sword hit count: " + currentHits);

            // Optional particles
            if (smashParticlePrefab != null)
            {
                ParticleSystem ps = Instantiate(
                    smashParticlePrefab,
                    hit.point + Vector3.up * 0.1f,
                    Quaternion.identity
                );

                var main = ps.main;
                Destroy(ps.gameObject, main.duration + main.startLifetimeMultiplier);
            }

            if (currentHits >= hitsToFix)
            {
                FixSword(hit.collider.gameObject);
            }
        }
    }

    private void FixSword(GameObject swordHit)
    {
        isFixed = true;
        Debug.Log("Sword fixed! (" + swordHit.name + ")");

        if (swordSpawn != null)
        {
            // Stop physics so it doesn't fall out of the hand
            Rigidbody rb = swordHit.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = swordHit.GetComponent<Collider>();
            if (col != null) col.enabled = false;   // optional

            // Parent to the hand and snap into place
            swordHit.transform.SetParent(swordSpawn);
            swordHit.transform.position = swordSpawn.position;
            swordHit.transform.rotation = swordSpawn.rotation;
        }
        else
        {
            Debug.LogWarning("HammerSwordHit: swordSpawn is not assigned.");
        }

        // Trigger final dialogue
        if (dialogue != null)
        {
            if (!triggerSwordFixedDialogueOnce || !swordFixedDialogueTriggered)
            {
                swordFixedDialogueTriggered = true;
                dialogue.PlayDialogue(swordFixedDialogueId);
            }
        }
        else
        {
            Debug.LogWarning("HammerSwordHit: Dialogue reference is missing.");
        }
    }
}
