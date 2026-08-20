using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class RulebookHingeController : MonoBehaviour
{
    [Header("Ρυθμίσεις Περιστροφής")]
    [Tooltip("Το αντικείμενο (μεντεσές) που ελέγχει το εξώφυλλο.")]
    public Transform bookCoverHinge;

    [Tooltip("Οι μοίρες (X,Y,Z) όταν το βιβλίο είναι ΚΛΕΙΣΤΟ.")]
    public Vector3 closedRotation = Vector3.zero;

    [Tooltip("Οι μοίρες (X,Y,Z) όταν το βιβλίο είναι ΑΝΟΙΧΤΟ. (π.χ. 0, 0, 180 ή 0, 0, -180)")]
    public Vector3 openRotation = new Vector3(0, 0, 180);

    [Tooltip("Πόσο γρήγορα γυρίζει η σελίδα/εξώφυλλο; (Μεγαλύτερο νούμερο = πιο γρήγορα)")]
    public float flipSpeed = 5f;

    [Header("Ήχος (Προαιρετικό)")]
    public AudioSource flipSound;

    private XRBaseInteractable interactable;
    private bool isOpen = false;
    private Coroutine flipCoroutine;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        // Σιγουρευόμαστε ότι ξεκινάει κλειστό
        if (bookCoverHinge != null)
        {
            bookCoverHinge.localEulerAngles = closedRotation;
        }
    }

    private void OnEnable()
    {
        // Όταν το κρατάς και πατάς σκανδάλη (Trigger)
        interactable.activated.AddListener(OnTriggerPressed);
    }

    private void OnDisable()
    {
        interactable.activated.RemoveListener(OnTriggerPressed);
    }

    private void OnTriggerPressed(ActivateEventArgs args)
    {
        ToggleBook();
    }

    public void ToggleBook()
    {
        isOpen = !isOpen;

        // Ήχος
        if (flipSound != null)
        {
            flipSound.Play();
        }

        // Σταματάμε την προηγούμενη κίνηση αν ήταν στη μέση και ξεκινάμε τη νέα
        if (flipCoroutine != null) StopCoroutine(flipCoroutine);
        flipCoroutine = StartCoroutine(FlipRoutine(isOpen ? openRotation : closedRotation));
    }

    // Η ρουτίνα που κάνει το άνοιγμα ομαλό (smooth)
    private IEnumerator FlipRoutine(Vector3 targetEulerAngles)
    {
        if (bookCoverHinge == null) yield break;

        Quaternion startRotation = bookCoverHinge.localRotation;
        Quaternion targetRotation = Quaternion.Euler(targetEulerAngles);
        float timeElapsed = 0f;

        while (timeElapsed < 1f)
        {
            timeElapsed += Time.deltaTime * flipSpeed;
            // Ομαλή μετάβαση (Spherical Interpolation)
            bookCoverHinge.localRotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed);
            yield return null;
        }

        // Κλείδωμα στην τελική θέση για ακρίβεια
        bookCoverHinge.localRotation = targetRotation;
    }
}