using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatingAway : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private GameObject dialogueTimeToLeave;   // your "DialogueTimeToLeave" trigger GO

    [Header("Timing")]
    [SerializeField] private float totalTime = 60f;
    [SerializeField] private float uiShowTime = 50f;

    [Header("UI")]
    [SerializeField] private GameObject leavingUI;             // panel or text parent
    [SerializeField] private TextMeshProUGUI leavingText;      // TMP text (optional)
    [SerializeField] private string leavingMessage = "Leaving shore captain saved succesfully!";

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "Main Menu";

    private bool sequenceStarted;

    private void Awake()
    {
        if (leavingUI != null)
            leavingUI.SetActive(false);
    }

    private void Update()
    {
        if (sequenceStarted) return;
        if (dialogueTimeToLeave == null) return;

        // Start when the trigger GameObject gets enabled (escort complete)
        if (!dialogueTimeToLeave.activeInHierarchy) return;

        sequenceStarted = true;
        StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        // Optional tiny delay so the dialogue can start displaying first
        yield return null;

        float waitToUI = Mathf.Clamp(uiShowTime, 0f, totalTime);
        yield return new WaitForSeconds(waitToUI);

        if (leavingUI != null)
            leavingUI.SetActive(true);

        if (leavingText != null)
            leavingText.text = leavingMessage;

        float remaining = Mathf.Max(0f, totalTime - waitToUI);
        yield return new WaitForSeconds(remaining);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
