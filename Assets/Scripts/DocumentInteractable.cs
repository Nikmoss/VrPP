using System.Collections;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Εξειδικευμένο Interactable για τα έγγραφα του παιχνιδιού.
/// Επιτρέπει πιάσιμο με ένα χέρι και χρησιμοποιεί τη σκανδάλη (Trigger/Activate)
/// για το άνοιγμα του εξωφύλλου μέσω κώδικα (Procedural Rotation), χωρίς Animator.
/// </summary>
public class DocumentInteractable : XRGrabInteractable
{
    [Header("Ρυθμίσεις Εξωφύλλου (Κώδικας)")]
    [Tooltip("Το Transform (Empty GameObject Pivot) του εξωφύλλου που θα περιστρέφεται.")]
    [SerializeField] private Transform coverPivot;

    [Tooltip("Η τοπική περιστροφή (στους άξονες X,Y,Z) όταν το διαβατήριο είναι ΚΛΕΙΣΤΟ.")]
    [SerializeField] private Vector3 closedRotation = Vector3.zero;

    [Tooltip("Η τοπική περιστροφή (στους άξονες X,Y,Z) όταν το διαβατήριο είναι ΑΝΟΙΧΤΟ.")]
    [SerializeField] private Vector3 openRotation = new Vector3(0, 0, 180);

    [Tooltip("Η ταχύτητα με την οποία ανοιγοκλείνει το εξώφυλλο (προτείνεται 10-15).")]
    [SerializeField] private float animationSpeed = 12f;

    [Tooltip("Προαιρετικός ήχος όταν ανοιγοκλείνει το χαρτί.")]
    [SerializeField] private AudioSource paperRustleSound;

    // Παρακολουθεί την τρέχουσα κατάσταση (ανοιχτό/κλειστό)
    private bool isDocumentOpen = false;

    // Αποθηκεύει την τρέχουσα Coroutine κίνησης για να μπορούμε να τη διακόψουμε αν χρειαστεί
    private Coroutine animationCoroutine;

    protected override void Awake()
    {
        base.Awake();

        // Επιβάλλουμε το κράτημα ΜΟΝΟ με ένα χέρι
        selectMode = InteractableSelectMode.Single;

        // Βεβαιωνόμαστε ότι το έγγραφο ξεκινάει στην κλειστή θέση
        if (coverPivot != null)
        {
            coverPivot.localRotation = Quaternion.Euler(closedRotation);
        }
    }

    /// <summary>
    /// Καλείται αυτόματα από το XR Toolkit όταν ο παίκτης πατήσει τη σκανδάλη.
    /// </summary>
    protected override void OnActivated(ActivateEventArgs args)
    {
        base.OnActivated(args);

        // Αντιστρέφουμε την κατάσταση
        isDocumentOpen = !isDocumentOpen;

        // Αν το εξώφυλλο κινείται ήδη, σταματάμε την προηγούμενη κίνηση
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        // Ξεκινάμε τη νέα κίνηση προς τη σωστή γωνία
        Vector3 targetAngles = isDocumentOpen ? openRotation : closedRotation;
        animationCoroutine = StartCoroutine(AnimateCoverRoutine(targetAngles));

        if (paperRustleSound != null)
        {
            paperRustleSound.Play();
        }

        Debug.Log(isDocumentOpen ? "Το έγγραφο άνοιξε (μέσω κώδικα)." : "Το έγγραφο έκλεισε (μέσω κώδικα).");
    }

    /// <summary>
    /// Coroutine που αναλαμβάνει την ομαλή περιστροφή του εξωφύλλου (Interpolation).
    /// </summary>
    private IEnumerator AnimateCoverRoutine(Vector3 targetEulerAngles)
    {
        if (coverPivot == null) yield break;

        Quaternion targetRotation = Quaternion.Euler(targetEulerAngles);

        // Συνεχίζουμε την περιστροφή όσο η διαφορά είναι μεγαλύτερη από 0.1 μοίρες
        while (Quaternion.Angle(coverPivot.localRotation, targetRotation) > 0.1f)
        {
            coverPivot.localRotation = Quaternion.Lerp(coverPivot.localRotation, targetRotation, Time.deltaTime * animationSpeed);

            // Περιμένουμε το επόμενο frame
            yield return null;
        }

        // Κλειδώνουμε ακριβώς στην τελική γωνία για ασφάλεια
        coverPivot.localRotation = targetRotation;
    }
}