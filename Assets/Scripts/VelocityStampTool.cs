using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Εργαλείο σφραγίδας. Ανιχνεύει τη σύγκρουση με το έγγραφο 
/// ΜΟΝΟ αν η ταχύτητα κίνησης ξεπερνά το όριο.
/// Περιλαμβάνει Αναλυτικό Διαγνωστικό Σύστημα για το Haptic Feedback.
/// </summary>
public class VelocityStampTool : MonoBehaviour
{
    public enum StampDecision { Approved, Denied }

    [Header("Ρυθμίσεις Απόφασης")]
    public StampDecision decisionType;

    [Tooltip("Το γραφικό (Prefab - Quad) που θα τυπωθεί πάνω στο χαρτί.")]
    [SerializeField] private GameObject stampMarkPrefab;

    [Tooltip("Ο ήχος της σφραγίδας (ΓΚΑΠ!).")]
    [SerializeField] private AudioSource stampAudioSource;

    [Tooltip("Ο ελάχιστος χρόνος μεταξύ σφραγισμάτων.")]
    [SerializeField] private float stampCooldown = 0.5f;

    [Header("Ρυθμίσεις Φυσικής & Ταχύτητας")]
    [Tooltip("Η ελάχιστη ταχύτητα (m/s) για να καταγραφεί ως χτύπημα. Προτείνεται 0.4")]
    [SerializeField] private float minimumVelocityThreshold = 0.4f;

    [Header("Ανάδραση: Δόνηση Χειριστηρίου (Haptics)")]
    [Tooltip("Ενεργοποιεί τη δόνηση στο χέρι που κρατάει τη σφραγίδα.")]
    [SerializeField] private bool enableHaptics = true;
    [Range(0f, 1f)]
    [Tooltip("Ένταση δόνησης (0 έως 1). Το 1 είναι η μέγιστη δύναμη.")]
    [SerializeField] private float hapticAmplitude = 0.8f;
    [Tooltip("Διάρκεια δόνησης σε δευτερόλεπτα.")]
    [SerializeField] private float hapticDuration = 0.15f;

    private float lastStampTime = 0f;
    private XRGrabInteractable grabInteractable;
    private Rigidbody stampRigidbody;

    private void Awake()
    {
        // Ψάχνουμε το XRGrabInteractable προς τα πάνω στην ιεραρχία
        grabInteractable = GetComponentInParent<XRGrabInteractable>();
        stampRigidbody = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastStampTime < stampCooldown) return;

        // Αν υπάρχει XRGrabInteractable, βεβαιωνόμαστε ότι το κρατάει ο παίκτης
        if (grabInteractable != null && !grabInteractable.isSelected) return;

        float currentVelocity = 0f;
        if (stampRigidbody != null)
        {
            currentVelocity = stampRigidbody.linearVelocity.magnitude;
        }

        if (currentVelocity < minimumVelocityThreshold)
        {
            return;
        }

        DocumentInteractable document = other.GetComponentInParent<DocumentInteractable>();
        if (document != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            ApplyStamp(document, hitPoint, other.transform);
        }
    }

    private void ApplyStamp(DocumentInteractable document, Vector3 hitPosition, Transform hitPageTransform)
    {
        lastStampTime = Time.time;

        if (stampAudioSource != null)
        {
            stampAudioSource.Play();
        }

        if (stampMarkPrefab != null)
        {
            GameObject newMark = Instantiate(stampMarkPrefab);
            newMark.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            newMark.transform.position = hitPosition + (hitPageTransform.up * 0.002f);
            newMark.transform.rotation = hitPageTransform.rotation * Quaternion.Euler(90f, 0f, 0f);
            newMark.transform.SetParent(hitPageTransform, true);
        }

        // Ενεργοποίηση Δόνησης με αναλυτικό έλεγχο
        TriggerHaptics();

        Debug.Log($"<color=green>Επιτυχές χτύπημα!</color> Τυπώθηκε: {decisionType}");

        CheckpointStateMachine stateMachine = FindObjectOfType<CheckpointStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.ChangeState(CheckpointStateMachine.CheckpointState.Stamped);
        }
    }

    /// <summary>
    /// Στέλνει εντολή δόνησης και τυπώνει αναλυτικά βήματα στην κονσόλα.
    /// </summary>
    private void TriggerHaptics()
    {
        Debug.Log("<color=yellow>HAPTICS:</color> Ξεκινάει ο έλεγχος δόνησης...");

        if (!enableHaptics)
        {
            Debug.LogWarning("<color=orange>HAPTICS ΑΚΥΡΩΣΗ:</color> Το κουτάκι 'Enable Haptics' είναι ξε-τικαρισμένο στον Inspector.");
            return;
        }

        if (grabInteractable == null)
        {
            Debug.LogError("<color=red>HAPTICS ΣΦΑΛΜΑ:</color> Το script δεν βρίσκει το XRGrabInteractable! Βεβαιώσου ότι το VelocityStampTool είναι παιδί του αντικειμένου που έχει το XRGrabInteractable.");
            return;
        }

        if (grabInteractable.interactorsSelecting.Count == 0)
        {
            Debug.LogWarning("<color=orange>HAPTICS ΑΚΥΡΩΣΗ:</color> Το αντικείμενο δεν φαίνεται να κρατιέται από κανένα χέρι αυτή τη στιγμή.");
            return;
        }

        bool foundPlayer = false;

        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            // Ψάχνουμε το HapticImpulsePlayer
            HapticImpulsePlayer hapticPlayer = interactor.transform.GetComponentInParent<HapticImpulsePlayer>();

            if (hapticPlayer == null)
            {
                hapticPlayer = interactor.transform.GetComponentInChildren<HapticImpulsePlayer>();
            }

            if (hapticPlayer != null)
            {
                hapticPlayer.SendHapticImpulse(hapticAmplitude, hapticDuration);
                Debug.Log("<color=cyan>HAPTICS ΕΠΙΤΥΧΙΑ:</color> Το σήμα δόνησης στάλθηκε στον Controller!");
                foundPlayer = true;
            }
            else
            {
                Debug.LogWarning($"<color=orange>HAPTICS:</color> Δεν βρέθηκε HapticImpulsePlayer στο χέρι: {interactor.transform.name}");
            }
        }

        if (!foundPlayer)
        {
            Debug.LogError("<color=red>HAPTICS ΣΦΑΛΜΑ:</color> ΔΕΝ βρέθηκε πουθενά το HapticImpulsePlayer.");
        }
    }
}