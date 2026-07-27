using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Ελέγχει δύο Sockets ώστε να λειτουργούν σαν "στοίβα". 
/// Το πάνω Socket δέχεται αντικείμενο ΜΟΝΟ αν το κάτω είναι γεμάτο.
/// Επίσης, αν η στοίβα είναι γεμάτη, το κάτω αντικείμενο ΚΛΕΙΔΩΝΕΙ μέχρι να πάρεις το πάνω.
/// </summary>
public class StackedSocketController : MonoBehaviour
{
    [Header("Ρυθμίσεις Στοίβας (Sockets)")]
    [Tooltip("Το Βασικό (Κάτω) Socket που πρέπει να γεμίσει πρώτο")]
    public XRSocketInteractor bottomSocket;

    [Tooltip("Το Δεύτερο (Πάνω) Socket")]
    public XRSocketInteractor topSocket;

    private GameObject lastBottomItem;
    private Collider[] bottomColliders;

    private void Update()
    {
        if (bottomSocket == null || topSocket == null) return;

        // 1. Ενεργοποίηση/Απενεργοποίηση του πάνω Socket
        // Επιτρέπεται να πάρει χαρτί μόνο αν το κάτω έχει ήδη χαρτί (ή αν έχουν γεννηθεί ταυτόχρονα)
        bool canUseTopSocket = bottomSocket.hasSelection || topSocket.hasSelection;
        if (topSocket.socketActive != canUseTopSocket)
        {
            topSocket.socketActive = canUseTopSocket;
        }

        // 2. Λογική "Κλειδώματος" του κάτω χαρτιού
        if (bottomSocket.hasSelection)
        {
            GameObject currentBottomItem = bottomSocket.GetOldestInteractableSelected().transform.gameObject;

            // Αν εντοπίσαμε νέο χαρτί στο κάτω socket, το αποθηκεύουμε
            if (lastBottomItem != currentBottomItem)
            {
                RestoreColliders(); // Σιγουρευόμαστε ότι το παλιό χαρτί (αν υπήρχε) ξεκλείδωσε

                lastBottomItem = currentBottomItem;
                // Βρίσκουμε όλα τα colliders του κάτω χαρτιού
                bottomColliders = lastBottomItem.GetComponentsInChildren<Collider>();
            }

            // ΕΔΩ ΕΙΝΑΙ ΤΟ ΜΥΣΤΙΚΟ:
            // Αν το πάνω Socket ΕΧΕΙ χαρτί, το κάτω κλειδώνει (τα colliders σβήνουν).
            // Αν το πάνω Socket ΕΙΝΑΙ ΑΔΕΙΟ, το κάτω ξεκλειδώνει (τα colliders ανάβουν).
            bool isTopFull = topSocket.hasSelection;
            SetCollidersEnabled(!isTopFull);
        }
        else
        {
            // Αν το κάτω socket είναι εντελώς άδειο, επαναφέρουμε τυχόν κλειδωμένα colliders 
            RestoreColliders();
            lastBottomItem = null;
            bottomColliders = null;
        }
    }

    // Μέθοδος που ανάβει/σβήνει την αφή (colliders)
    private void SetCollidersEnabled(bool state)
    {
        if (bottomColliders == null) return;

        foreach (var col in bottomColliders)
        {
            if (col != null && col.enabled != state)
            {
                col.enabled = state;
            }
        }
    }

    // Μέθοδος ασφαλείας για να μην μείνει ποτέ ένα χαρτί μόνιμα αόρατο στα χέρια σου
    private void RestoreColliders()
    {
        if (lastBottomItem != null && bottomColliders != null)
        {
            foreach (var col in bottomColliders)
            {
                if (col != null) col.enabled = true;
            }
        }
    }

    // Αν κλείσει το παιχνίδι ή το script, ξεκλειδώνουμε τα πάντα
    private void OnDisable()
    {
        RestoreColliders();
    }
}