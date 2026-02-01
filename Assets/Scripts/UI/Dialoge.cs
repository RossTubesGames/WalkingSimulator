using System.Collections;
using TMPro;
using UnityEngine;

public class Dialoge : MonoBehaviour
{
    [System.Serializable]
    public class DialogueSet
    {
        public string id;              // e.g. "Intro", "KeyFound"
        [TextArea] public string[] lines;
    }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textComponent;

    [Header("Typing")]
    [SerializeField] private float textSpeed = 0.03f;

    [Header("Dialogue Sets")]
    [SerializeField] private DialogueSet[] sets;

    [Header("Start Behaviour")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private string startId = "Intro";

    private string[] currentLines;
    private int index;
    private bool isPlaying;

    private void Start()
    {
        if (textComponent != null)
            textComponent.gameObject.SetActive(false);

        if (playOnStart)
            PlayDialogue(startId);
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == currentLines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = currentLines[index];
            }
        }
    }

    // Call this from anywhere when an objective happens
    public void PlayDialogue(string id)
    {
        var lines = FindLines(id);
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("Dialogue id not found or empty: " + id);
            return;
        }

        currentLines = lines;
        index = 0;
        isPlaying = true;

        textComponent.gameObject.SetActive(true);
        textComponent.text = string.Empty;

        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    // Convenience method if you want to call "KeyFound" without typing the string elsewhere
    public void PlayKeyFoundDialogue()
    {
        PlayDialogue("KeyFound");
    }

    private string[] FindLines(string id)
    {
        for (int i = 0; i < sets.Length; i++)
        {
            if (sets[i].id == id)
                return sets[i].lines;
        }
        return null;
    }

    private IEnumerator TypeLine()
    {
        foreach (char c in currentLines[index])
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void NextLine()
    {
        if (index < currentLines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;

            StopAllCoroutines();
            StartCoroutine(TypeLine());
        }
        else
        {
            isPlaying = false;
            textComponent.text = string.Empty;
            textComponent.gameObject.SetActive(false);
        }
    }
}
