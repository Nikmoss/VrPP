using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Εργαλείο σφραγίδας. Ανιχνεύει τη σύγκρουση, παίζει haptics, 
/// τυπώνει το σωστό γραφικό ανάλογα με το μελάνι και καταγράφει την απόφαση.
/// </summary>
public class VelocityStampTool : MonoBehaviour
{
    public enum StampDecision { None, Approved, Denied }

    [Header("Κατάσταση Σφραγίδας")]
    [Tooltip("Το τρέχον μελάνι που έχει πάρει η σφραγίδα. Πρέπει να τη βουτήξεις!")]
    public StampDecision currentInk = StampDecision.None;

    [Header("Οπτική Ένδειξη Μελανιού (Σφουγγαράκι)")]
    [SerializeField] private MeshRenderer stampTipRenderer;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material approvedMaterial;
    [SerializeField] private Material deniedMaterial;

    [Header("Γραφικά Σφραγίδας (Decals στο χαρτί)")]
    [Tooltip("Το γραφικό που θα τυπωθεί για ΕΓΚΡΙΣΗ (π.χ. πράσινο/μαύρο).")]
    [SerializeField] private GameObject approvedMarkPrefab;
    [Tooltip("Το γραφικό που θα τυπωθεί για ΑΠΟΡΡΙΨΗ (π.χ. κόκκινο).")]
    [SerializeField] private GameObject deniedMarkPrefab;

    [Header("Ήχος & Cooldown")]
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

    // --- ΝΕΑ ΜΕΘΟΔΟΣ ΓΙΑ ΤΟ ΤΑΜΠΟΝ ---
    // Καλείται από το script InkPadZone όταν ακουμπάς το μελάνι
    public void DipInInk(StampDecision newInk)
    {
        currentInk = newInk;

        if (stampTipRenderer != null)
        {
            if (currentInk == StampDecision.Approved && approvedMaterial != null)
                stampTipRenderer.material = approvedMaterial;
            else if (currentInk == StampDecision.Denied && deniedMaterial != null)
                stampTipRenderer.material = deniedMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Αν δεν έχει πάρει μελάνι (None), δεν μπορεί να σφραγίσει τίποτα!
        if (currentInk == StampDecision.None) return;

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

        // --- ΕΠΙΛΟΓΗ ΣΩΣΤΟΥ ΓΡΑΦΙΚΟΥ (DECAL) ---
        GameObject prefabToInstantiate = (currentInk == StampDecision.Approved) ? approvedMarkPrefab : deniedMarkPrefab;

        if (prefabToInstantiate != null)
        {
            GameObject newMark = Instantiate(prefabToInstantiate);
            newMark.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            newMark.transform.position = hitPosition + (hitPageTransform.up * 0.002f);
            newMark.transform.rotation = hitPageTransform.rotation * Quaternion.Euler(90f, 0f, 0f);
            newMark.transform.SetParent(hitPageTransform, true);
        }

        TriggerHaptics();

        NPCController currentNPC = FindObjectOfType<NPCController>();
        if (currentNPC != null)
        {
            currentNPC.DocumentWasStamped();
        }

        DynamicPassport dynamicPassport = document.GetComponent<DynamicPassport>();
        if (dynamicPassport != null)
        {
            dynamicPassport.SetStampDecision(currentInk);
        }

        MerchantPermit merchantPermit = document.GetComponent<MerchantPermit>();
        if (merchantPermit != null)
        {
            merchantPermit.SetStampDecision(currentInk);
        }

        // --- ΑΔΕΙΑΣΜΑ ΜΕΛΑΝΙΟΥ ΓΙΑ ΡΕΑΛΙΣΜΟ ---
        // Ο παίκτης πρέπει να ξαναβουτήξει τη σφραγίδα για το επόμενο χαρτί!
        currentInk = StampDecision.None;
        if (stampTipRenderer != null && defaultMaterial != null)
        {
            stampTipRenderer.material = defaultMaterial;
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