using UnityEngine;

public class DialogueTriggers : MonoBehaviour
{
    [SerializeField] private Dialoge dialogue;
    [SerializeField] private string dialogueId = "ShipSight";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void Awake()
    {
        if (dialogue == null)
            dialogue = FindObjectOfType<Dialoge>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Dialogue trigger entered by: " + other.name + " (Tag: " + other.tag + ")");

        if (triggerOnce && hasTriggered)
            return;

        // Sometimes the collider is a child; check root tag too
        bool tagMatch = other.CompareTag(playerTag);
        if (!tagMatch && other.transform.root != null)
            tagMatch = other.transform.root.CompareTag(playerTag);

        if (!tagMatch)
        {
            Debug.Log("Not player, ignoring.");
            return;
        }

        hasTriggered = true;

        if (dialogue == null)
        {
            Debug.LogWarning("No Dialoge found in scene.");
            return;
        }

        Debug.Log("Playing dialogue id: " + dialogueId);
        dialogue.PlayDialogue(dialogueId);
    }
}