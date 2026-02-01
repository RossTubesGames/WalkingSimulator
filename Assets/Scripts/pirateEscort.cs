using UnityEngine;
using UnityEngine.AI;

public class pirateEscort : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform shipDestination;
    public Transform swordSpawn;
    public GameObject BackgroundMusic;
    public GameObject EscortMusic;
    public GameObject DialogueTimeToLeave; // your "timetoleave" trigger object

    [Header("Skybox + Lights")]
    [SerializeField] private Material escortSkybox;
    [SerializeField] private GameObject dayLight;     // drag your DayLight object here
    [SerializeField] private GameObject nightLight;   // drag your NightLight object here

    [Header("Dialogue Trigger (On Destination Reached)")]
    [SerializeField] private Dialoge dialogue;
    [SerializeField] private string destinationReachedDialogueId = "DestinationReached";
    [SerializeField] private bool triggerDialogueOnce = true;

    [Header("Behaviour Settings")]
    public float followDistance = 3f;
    public float stopDistanceToShip = 2f;
    public float updateInterval = 0.2f;

    private NavMeshAgent agent;
    private float nextUpdateTime;
    private bool escortActive = false;
    private bool escortFinished = false;

    private bool destinationDialogueTriggered = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("PirateEscort: No NavMeshAgent found on " + gameObject.name);

        if (dialogue == null)
            dialogue = FindObjectOfType<Dialoge>();

        // Make sure the trigger starts disabled
        if (DialogueTimeToLeave != null)
            DialogueTimeToLeave.SetActive(false);
    }

    void Update()
    {
        if (agent == null) return;

        // Activate escort once pirate has the sword in his hand
        if (!escortActive && PirateHasSword())
        {
            escortActive = true;

            if (BackgroundMusic != null) BackgroundMusic.SetActive(false);
            if (EscortMusic != null) EscortMusic.SetActive(true);

            if (escortSkybox != null)
            {
                RenderSettings.skybox = escortSkybox;
                DynamicGI.UpdateEnvironment();
            }

            // Swap lights (this is the part you want)
            if (dayLight != null) dayLight.SetActive(false);
            if (nightLight != null) nightLight.SetActive(true);

            Debug.Log("PirateEscort: Escort started – pirate has the sword.");
        }

        if (!escortActive || escortFinished) return;

        // Have we reached the ship?
        float distToShip = Vector3.Distance(transform.position, shipDestination.position);
        if (distToShip <= stopDistanceToShip)
        {
            escortFinished = true;
            agent.ResetPath();
            Debug.Log("PirateEscort: Escort finished – reached the ship.");

            // Enable the "TimeToLeave" trigger so it can fire its own dialogue
            if (DialogueTimeToLeave != null)
                DialogueTimeToLeave.SetActive(true);

            // Optional: also play an immediate dialogue here
            if (dialogue != null)
            {
                if (!triggerDialogueOnce || !destinationDialogueTriggered)
                {
                    destinationDialogueTriggered = true;
                    dialogue.PlayDialogue(destinationReachedDialogueId);
                }
            }
            else
            {
                Debug.LogWarning("pirateEscort: Dialogue reference is missing.");
            }

            return;
        }

        // Follow the player, but only update path every interval
        if (Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;

            float distToPlayer = Vector3.Distance(transform.position, player.position);

            if (distToPlayer > followDistance)
                agent.SetDestination(player.position);
            else
                agent.ResetPath();
        }
    }

    private bool PirateHasSword()
    {
        return swordSpawn != null && swordSpawn.childCount > 0;
    }
}
