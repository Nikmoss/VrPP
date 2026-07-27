using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
// Αν σου βγάλει error για το XRSocketInteractor, βγάλε τα "//" από την επόμενη γραμμή:
// using UnityEngine.XR.Interaction.Toolkit.Interactors; 

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC References")]
    public GameObject npcPrefab;
    public GameObject passportPrefab;
    public GameObject merchantPermitPrefab; // ΝΕΟ: Το Prefab της άδειας

    [Header("Waypoints")]
    public Transform spawnPoint;
    public Transform windowPoint;
    public Transform exitPoint;

    [Header("Desk Setup (Client Sockets)")]
    public XRSocketInteractor primarySocket; // ΝΕΟ: Το βασικό socket για το διαβατήριο
    public XRSocketInteractor secondarySocket; // ΝΕΟ: Το δεύτερο socket για την άδεια

    [Header("Timing & Settings")]
    public float spawnDelay = 1.0f; // Ο χρόνος καθυστέρησης
    [Range(0f, 1f)] public float merchantProbability = 0.5f; // 50% πιθανότητα να είναι έμπορος

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

        // 2. Ελέγχουμε αν βρισκόμαστε ήδη στη φάση αναμονής
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

        // Η παύση
        yield return new WaitForSeconds(spawnDelay);

        // Δημιουργία του νέου NPC
        currentNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);

        NPCController controller = currentNPC.GetComponent<NPCController>();
        if (controller != null)
        {
            // Ρίχνουμε το "ζάρι" για να δούμε αν είναι έμπορος
            bool isMerchant = Random.value < merchantProbability;

            GameObject[] docsToSpawn;
            XRSocketInteractor[] socketsToUse;

            if (isMerchant && merchantPermitPrefab != null && secondarySocket != null)
            {
                // Φέρνει και τα 2 χαρτιά
                docsToSpawn = new GameObject[] { passportPrefab, merchantPermitPrefab };
                socketsToUse = new XRSocketInteractor[] { primarySocket, secondarySocket };
            }
            else
            {
                // Φέρνει μόνο το διαβατήριο
                docsToSpawn = new GameObject[] { passportPrefab };
                socketsToUse = new XRSocketInteractor[] { primarySocket };
            }

            // Στέλνουμε τις λίστες στον NPCController
            controller.Setup(spawnPoint, windowPoint, exitPoint, docsToSpawn, socketsToUse);
        }

        isSpawning = false;
    }
}