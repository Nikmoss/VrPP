using UnityEngine;
using System.Collections;

/// <summary>
/// Ο κεντρικός διαχειριστής της ροής του παιχνιδιού.
/// Ελέγχει σε ποια φάση βρίσκεται η βάρδια.
/// </summary>
public class CheckpointStateMachine : MonoBehaviour
{
    public enum CheckpointState
    {
        WaitingForNPC,      // Το γκισέ είναι άδειο
        DocumentsSpawned,   // Τα χαρτιά βρίσκονται στο γραφείο
        Inspecting,         // Ο παίκτης εξετάζει/κρατάει τα έγγραφα
        DocumentOnDesk,     // Το διαβατήριο είναι στο Stamping Pad
        Stamped,            // Μπήκε η σφραγίδα (Τέλος ελέγχου)
        NPCReacting,        // Παύση έντασης (Tension Pause)
        NPCLeaving          // Ο NPC φεύγει
    }

    [Header("Κατάσταση Παιχνιδιού")]
    [SerializeField] private CheckpointState currentState = CheckpointState.WaitingForNPC;

    // Δημόσιο property για να διαβάζουν άλλα scripts την κατάσταση
    public CheckpointState CurrentState => currentState;

    private void Start()
    {
        Debug.Log("<color=green>Το Checkpoint είναι ανοιχτό. Η βάρδια ξεκινά.</color>");
        ChangeState(CheckpointState.WaitingForNPC);
    }

    /// <summary>
    /// Μέθοδος για την αλλαγή της κατάστασης του παιχνιδιού.
    /// Κανονικά θα καλείται από τις σφραγίδες, τα έγγραφα ή τον NPC Spawner.
    /// </summary>
    public void ChangeState(CheckpointState newState)
    {
        if (currentState == newState) return;

        Debug.Log($"<color=cyan>Αλλαγή Κατάστασης:</color> Από {currentState} σε {newState}");
        currentState = newState;

        // Εκτέλεση λογικής ανάλογα με τη νέα κατάσταση
        switch (currentState)
        {
            case CheckpointState.WaitingForNPC:
                // TODO: Κλήση του επόμενου NPC
                break;
            case CheckpointState.DocumentsSpawned:
                // TODO: Εμφάνιση εγγράφων
                break;
            case CheckpointState.DocumentOnDesk:
                Debug.Log("Το έγγραφο είναι στη θέση του. Έτοιμο για σφράγιση.");
                break;
            case CheckpointState.Stamped:
                Debug.Log("Η απόφαση πάρθηκε. Κλείδωμα εγγράφων.");
                ChangeState(CheckpointState.NPCReacting);
                break;
            case CheckpointState.NPCReacting:
                StartCoroutine(TensionPauseRoutine());
                break;
            case CheckpointState.NPCLeaving:
                // TODO: Καθαρισμός γραφείου, αποχώρηση NPC
                break;
        }
    }

    /// <summary>
    /// Η δραματική παύση 1 δευτερολέπτου αφού σφραγίσεις.
    /// </summary>
    private IEnumerator TensionPauseRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log("Η αντίδραση του NPC ολοκληρώθηκε.");
        ChangeState(CheckpointState.NPCLeaving);
    }
}