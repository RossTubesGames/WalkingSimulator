using UnityEngine;

public class KeyFishUp : MonoBehaviour
{
    [Header("Connections")]
    public RodCast rod;                 // assign your RodCast on the rod
    public GameObject keyObject;        // disabled key object in scene (or prefab placed inactive)
    public int castsNeeded = 3;         // how many casts before key appears

    [Header("Dialogue")]
    [SerializeField] private Dialoge dialogue;
    [SerializeField] private string keyFoundDialogueId = "KeyFound";

    [Header("Attach-to-magnet (when found)")]
    public Vector3 keyLocalOffset;      // offset while stuck to magnet
    public Vector3 keyLocalEuler;

    [Header("Jump-to-spot after reel-in")]
    public Transform keyLandingSpot;    // where the key should jump to
    public float jumpTime = 0.7f;       // seconds
    public float jumpHeight = 1.5f;     // arc peak height

    private int casts;
    private bool keySpawned;
    private bool jumpQueued;            // set true once we've attached the key
    private Transform currentMagnet;    // cached when key attaches

    void Start()
    {
        if (!rod) rod = FindObjectOfType<RodCast>();

        if (dialogue == null)
            dialogue = FindObjectOfType<Dialoge>();

        if (rod != null)
        {
            rod.CastOutCompleted += OnCastOutCompleted;
            rod.ReelInCompleted += OnReelInCompleted;
        }

        if (keyObject != null)
            keyObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (rod != null)
        {
            rod.CastOutCompleted -= OnCastOutCompleted;
            rod.ReelInCompleted -= OnReelInCompleted;
        }
    }

    private void OnCastOutCompleted()
    {
        casts++;

        if (!keySpawned && casts >= castsNeeded)
        {
            keySpawned = true;

            var magnetGO = rod != null ? rod.CurrentMagnet : null;
            if (!magnetGO)
            {
                Debug.LogWarning("KeyFishUp: Magnet not available when trying to attach key.");
                return;
            }

            currentMagnet = magnetGO.transform;

            // Enable and attach key
            keyObject.SetActive(true);

            // Start dialogue exactly when key spawns
            if (dialogue != null && !string.IsNullOrEmpty(keyFoundDialogueId))
                dialogue.PlayDialogue(keyFoundDialogueId);

            keyObject.transform.SetParent(currentMagnet, true);
            keyObject.transform.localPosition = keyLocalOffset;
            keyObject.transform.localRotation = Quaternion.Euler(keyLocalEuler);

            // Silence physics while attached to magnet
            if (keyObject.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            jumpQueued = true;
        }
    }

    private void OnReelInCompleted()
    {
        if (!jumpQueued || !keyObject || !keyLandingSpot) return;

        keyObject.transform.SetParent(null, true);

        if (keyObject.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        StartCoroutine(JumpToSpot(
            keyObject.transform,
            keyObject.transform.position,
            keyLandingSpot.position,
            jumpTime,
            jumpHeight,
            onDone: () =>
            {
                if (keyObject.TryGetComponent<Rigidbody>(out var rb2))
                {
                    rb2.isKinematic = false;
                    rb2.useGravity = true;
                    rb2.detectCollisions = true;
                    rb2.linearVelocity = Vector3.zero;
                    rb2.angularVelocity = Vector3.zero;
                }

                keyObject.transform.rotation = keyLandingSpot.rotation;

                jumpQueued = false;
                currentMagnet = null;
            }
        ));
    }

    private System.Collections.IEnumerator JumpToSpot(Transform t, Vector3 start, Vector3 end, float duration, float height, System.Action onDone)
    {
        float elapsed = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float u = Mathf.Clamp01(elapsed / duration);

            Vector3 pos = Vector3.Lerp(start, end, u);
            pos.y += 4f * height * u * (1f - u);

            t.position = pos;
            yield return null;
        }

        t.position = end;
        onDone?.Invoke();
    }
}
