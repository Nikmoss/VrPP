using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Εργαλείο σφραγίδας. Ανιχνεύει τη σύγκρουση, παίζει haptics
/// και καταγράφει την απόφαση πάνω στο έγγραφο (χωρίς να δίνει άμεσα σκορ).
/// </summary>
public class VelocityStampTool : MonoBehaviour
{
    public enum StampDecision { Approved, Denied }

    [Header("Ρυθμίσεις Απόφασης")]
    public StampDecision decisionType;

    [Tooltip("Το γραφικό (Prefab - Quad) που θα τυπωθεί πάνω στο χαρτί.")]
    [SerializeField] private GameObject stampMarkPrefab;

    [SerializeField] private AudioSource stampAudioSource;
    [SerializeField] private float stampCooldown = 0.5f;

    [Header("Ρυθμίσεις Φυσικής & Ταχύτητας")]
    [SerializeField] private float minimumVelocityThreshold = 0.4f;

    [Header("Ανάδραση: Δόνηση Χειριστηρίου (Haptics)")]
    [SerializeField] private bool enableHaptics = true;
    [Range(0f, 1f)][SerializeField] private float hapticAmplitude = 0.8f;
    [SerializeField] private float hapticDuration = 0.15f;

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

        if (currentVelocity < minimumVelocityThreshold) return;

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

        if (stampAudioSource != null) stampAudioSource.Play();

        if (stampMarkPrefab != null)
        {
            GameObject newMark = Instantiate(stampMarkPrefab);
            newMark.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            newMark.transform.position = hitPosition + (hitPageTransform.up * 0.002f);
            newMark.transform.rotation = hitPageTransform.rotation * Quaternion.Euler(90f, 0f, 0f);
            newMark.transform.SetParent(hitPageTransform, true);
        }

        TriggerHaptics();

        // Ειδοποίηση του NPC ότι έπεσε σφραγίδα (για να ξέρει ότι μπορεί να πάρει το χαρτί αν μπει στο Socket)
        NPCController currentNPC = FindObjectOfType<NPCController>();
        if (currentNPC != null)
        {
            currentNPC.DocumentWasStamped();
        }

        // --- ΝΕΟ: Απλά καταγράφουμε την απόφαση στο χαρτί! ---
        DynamicPassport dynamicPassport = document.GetComponent<DynamicPassport>();
        if (dynamicPassport != null)
        {
            dynamicPassport.SetStampDecision(decisionType);
        }
    }

    private void TriggerHaptics()
    {
        if (!enableHaptics || grabInteractable == null) return;
        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            HapticImpulsePlayer hapticPlayer = interactor.transform.GetComponentInParent<HapticImpulsePlayer>();
            if (hapticPlayer == null) hapticPlayer = interactor.transform.GetComponentInChildren<HapticImpulsePlayer>();
            if (hapticPlayer != null) hapticPlayer.SendHapticImpulse(hapticAmplitude, hapticDuration);
        }
    }
}