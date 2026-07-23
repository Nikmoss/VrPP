using UnityEngine;

/// <summary>
/// Ελέγχει την απόσταση του εγγράφου από το VR Headset του παίκτη.
/// Αν έρθει αρκετά κοντά, εμφανίζει ομαλά (Fade In) το καθαρό UI ανάγνωσης.
/// </summary>
public class DocumentProximityUI : MonoBehaviour
{
    [Header("Ρυθμίσεις UI")]
    [Tooltip("Το Canvas Group του UI που θέλουμε να εμφανίζεται.")]
    [SerializeField] private CanvasGroup uiCanvasGroup;

    [Tooltip("Πόσο γρήγορα γίνεται το Fade In/Out;")]
    [SerializeField] private float fadeSpeed = 5f;

    [Header("Ρυθμίσεις Απόστασης")]
    [Tooltip("Η απόσταση (σε μέτρα) από το πρόσωπο όπου αρχίζει να εμφανίζεται το UI.")]
    [SerializeField] private float readDistanceThreshold = 0.4f;

    private Transform playerHeadset;
    private bool isCloseEnough = false;

    private void Start()
    {
        // Βρίσκουμε αυτόματα την κάμερα του παίκτη (VR Headset)
        if (Camera.main != null)
        {
            playerHeadset = Camera.main.transform;
        }
        else
        {
            Debug.LogError("Δεν βρέθηκε Main Camera στη σκηνή για το Proximity UI!");
        }

        // Βεβαιωνόμαστε ότι το UI ξεκινάει αόρατο
        if (uiCanvasGroup != null)
        {
            uiCanvasGroup.alpha = 0f;
            uiCanvasGroup.interactable = false;
            uiCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (playerHeadset == null || uiCanvasGroup == null) return;

        // Υπολογίζουμε την απόσταση από το κέντρο του χαρτιού μέχρι το πρόσωπο
        float distanceToFace = Vector3.Distance(transform.position, playerHeadset.position);

        // Αν είναι πιο κοντά από το όριο (π.χ. 40cm), το θεωρούμε "κοντά"
        isCloseEnough = distanceToFace <= readDistanceThreshold;

        // Ομαλή μετάβαση του Alpha (Διαφάνεια)
        float targetAlpha = isCloseEnough ? 1f : 0f;
        uiCanvasGroup.alpha = Mathf.Lerp(uiCanvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }
}