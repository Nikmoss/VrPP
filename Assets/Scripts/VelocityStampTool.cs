using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Εργαλείο σφραγίδας. Ανιχνεύει τη σύγκρουση με το έγγραφο 
/// ΜΟΝΟ αν η ταχύτητα κίνησης ξεπερνά το όριο.
/// Διορθωμένο Scale: Το Unity υπολογίζει αυτόματα την παραμόρφωση του Parent.
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

    private float lastStampTime = 0f;
    private XRGrabInteractable grabInteractable;
    private Rigidbody stampRigidbody;

    private void Awake()
    {
        grabInteractable = GetComponentInParent<XRGrabInteractable>();
        stampRigidbody = GetComponentInParent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Time.time - lastStampTime < stampCooldown) return;

        if (grabInteractable != null && !grabInteractable.isSelected) return;

        float currentVelocity = 0f;
        if (stampRigidbody != null)
        {
            currentVelocity = stampRigidbody.linearVelocity.magnitude;
        }

        if (currentVelocity < minimumVelocityThreshold)
        {
            Debug.Log($"Ταχύτητα {currentVelocity} m/s: Χρειάζεται πιο δυνατό χτύπημα.");
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
            // 1. Δημιουργούμε τη στάμπα εντελώς ελεύθερη (χωρίς parent ακόμα)
            GameObject newMark = Instantiate(stampMarkPrefab);

            // 2. Ορίζουμε το ΑΠΟΛΥΤΟ μέγεθος στον κόσμο (5x5 εκατοστά)
            newMark.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            // 3. Τη βάζουμε στη σωστή θέση (ελάχιστα πιο πάνω από το χαρτί για το Z-Fighting)
            newMark.transform.position = hitPosition + (hitPageTransform.up * 0.002f);

            // 4. Την περιστρέφουμε για να 'ξαπλώσει' και να ευθυγραμμιστεί με το χαρτί
            newMark.transform.rotation = hitPageTransform.rotation * Quaternion.Euler(90f, 0f, 0f);

            // 5. ΤΩΡΑ την κάνουμε παιδί της σελίδας.
            // Το "true" λέει στο Unity: "Κάνε ό,τι περίεργα μαθηματικά θες στο Local Scale, 
            // αρκεί να μου κρατήσεις τη στάμπα στο μέγεθος και τη θέση που την έβαλα μόλις τώρα".
            newMark.transform.SetParent(hitPageTransform, true);
        }

        Debug.Log($"<color=green>Επιτυχές χτύπημα!</color> Τυπώθηκε: {decisionType}");

        CheckpointStateMachine stateMachine = FindObjectOfType<CheckpointStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.ChangeState(CheckpointStateMachine.CheckpointState.Stamped);
        }
    }
}