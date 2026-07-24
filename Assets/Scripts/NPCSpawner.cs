using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC References")]
    public GameObject npcPrefab;
    public GameObject passportPrefab;

    [Header("Waypoints")]
    public Transform spawnPoint;
    public Transform windowPoint;
    public Transform exitPoint;

    [Header("Desk Setup")]
    public XRSocketInteractor deskSocket;

    [Header("Timing")]
    public float spawnDelay = 1.0f; // Ο χρόνος καθυστέρησης (1 δευτερόλεπτο)

    private GameObject currentNPC;
    private bool isSpawning = false; // Ασφάλεια για να μην σπαμάρουμε πελάτες

    public void SpawnNextNPC()
    {
        // 1. Ελέγχουμε αν υπάρχει ήδη πελάτης στο γκισέ
        if (currentNPC != null)
        {
            Debug.Log("Υπάρχει ήδη κάποιος στο γκισέ! Πρέπει να φύγει πρώτα.");
            return;
        }

        // 2. Ελέγχουμε αν βρισκόμαστε ήδη στη φάση αναμονής (για να μην διπλο-καλεστεί)
        if (isSpawning)
        {
            return;
        }

        // Ξεκινάμε τη διαδικασία με την καθυστέρηση
        StartCoroutine(SpawnWithDelayRoutine());
    }

    private IEnumerator SpawnWithDelayRoutine()
    {
        isSpawning = true;

        // Η παύση: Περιμένουμε τον χρόνο που ορίσαμε (1 δευτερόλεπτο)
        yield return new WaitForSeconds(spawnDelay);

        // Δημιουργία του νέου NPC
        currentNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);

        NPCController controller = currentNPC.GetComponent<NPCController>();
        if (controller != null)
        {
            controller.Setup(spawnPoint, windowPoint, exitPoint, passportPrefab, deskSocket);
        }

        isSpawning = false;
    }
}