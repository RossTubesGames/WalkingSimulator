using UnityEngine;

public class Emotes : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isEmotingBool = "isEmoting";
    [SerializeField] private string breakdanceTrigger = "Emote_Breakdance";
    [SerializeField] private float emoteDuration = 2.5f;

    [Header("Cameras")]
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private Camera emoteCamera;

    [Header("Disable During Emote")]
    [SerializeField] private MonoBehaviour playerCamScript;
    [SerializeField] private MonoBehaviour playerMovementScript;

    private float timer;
    private bool isPlaying;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        SetEmoteMode(false);
    }

    private void Update()
    {
        if (!isPlaying && (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)))
        {
            isPlaying = true;
            timer = emoteDuration;

            animator.SetBool(isEmotingBool, true);
            animator.SetTrigger(breakdanceTrigger);

            SetEmoteMode(true);
        }

        if (isPlaying)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isPlaying = false;
                animator.SetBool(isEmotingBool, false);

                SetEmoteMode(false);
            }
        }
    }

    private void SetEmoteMode(bool emoteMode)
    {
        // Cameras
        if (gameplayCamera != null)
            gameplayCamera.enabled = !emoteMode;

        if (emoteCamera != null)
            emoteCamera.enabled = emoteMode;

        // Prevent "two Audio Listeners" warning if both cameras have one
        if (gameplayCamera != null && gameplayCamera.TryGetComponent<AudioListener>(out var gListener))
            gListener.enabled = !emoteMode;

        if (emoteCamera != null && emoteCamera.TryGetComponent<AudioListener>(out var eListener))
            eListener.enabled = emoteMode;

        // Disable/Enable player control scripts
        if (playerCamScript != null)
            playerCamScript.enabled = !emoteMode;

        if (playerMovementScript != null)
            playerMovementScript.enabled = !emoteMode;
    }
}
