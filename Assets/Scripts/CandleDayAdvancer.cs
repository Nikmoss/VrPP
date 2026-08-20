using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class CandleDayAdvancer : MonoBehaviour
{
    [Header("Οπτικά Κεριού")]
    [Tooltip("Το GameObject που περιέχει τα Particles της φωτιάς")]
    public GameObject flameParticles;
    [Tooltip("Το Φως που ρίχνει το κερί στο γραφείο")]
    public Light candleLight;

    [Header("Ρυθμίσεις Μετάβασης")]
    [Tooltip("Πόσα δευτερόλεπτα θα μείνει σβηστό το κερί (και το δωμάτιο σκοτεινό)")]
    public float transitionDelay = 3.0f;

    [Header("Επαναφορά Εργαλείων (Reset)")]
    [Tooltip("Βάλε εδώ το Rulebook, τη Σφραγίδα, το Μαχαίρι κλπ. για να γυρνάνε στη θέση τους!")]
    public Transform[] toolsToReset;

    private Vector3[] initialToolPositions;
    private Quaternion[] initialToolRotations;

    private XRBaseInteractable interactable;
    private bool isTransitioning = false;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
    }

    private void Start()
    {
        initialToolPositions = new Vector3[toolsToReset.Length];
        initialToolRotations = new Quaternion[toolsToReset.Length];

        for (int i = 0; i < toolsToReset.Length; i++)
        {
            if (toolsToReset[i] != null)
            {
                initialToolPositions[i] = toolsToReset[i].position;
                initialToolRotations[i] = toolsToReset[i].rotation;
            }
        }
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnFlameGrabbed);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnFlameGrabbed);
    }

    private void OnFlameGrabbed(SelectEnterEventArgs args)
    {
        if (!isTransitioning)
        {
            StartCoroutine(DayTransitionRoutine());
        }
    }

    private IEnumerator DayTransitionRoutine()
    {
        isTransitioning = true;

        if (flameParticles != null) flameParticles.SetActive(false);
        if (candleLight != null) candleLight.enabled = false;

        yield return new WaitForSeconds(transitionDelay);

        if (DayManager.Instance != null)
        {
            DayManager.Instance.NextDay();
        }

        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetDailyStats();
        }

        NPCSpawner spawner = FindObjectOfType<NPCSpawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        // Καλούμε τη δημόσια μέθοδο για να καθαρίσει το γραφείο!
        ResetAllTools();

        if (flameParticles != null) flameParticles.SetActive(true);
        if (candleLight != null) candleLight.enabled = true;

        isTransitioning = false;
    }

    /// <summary>
    /// Επαναφέρει όλα τα εργαλεία στην αρχική τους θέση.
    /// Είναι public για να μπορεί να κληθεί και από άλλα κουμπιά (π.χ. Emergency Reset).
    /// </summary>
    public void ResetAllTools()
    {
        for (int i = 0; i < toolsToReset.Length; i++)
        {
            if (toolsToReset[i] != null)
            {
                toolsToReset[i].position = initialToolPositions[i];
                toolsToReset[i].rotation = initialToolRotations[i];

                Rigidbody rb = toolsToReset[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}