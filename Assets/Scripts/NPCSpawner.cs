using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class NPCSpawner : MonoBehaviour
{
    [Header("NPC References")]
    public GameObject npcPrefab;
    public GameObject passportPrefab;
    public GameObject merchantPermitPrefab;
    public GameObject mercenaryWritPrefab;

    [Header("Waypoints")]
    public Transform spawnPoint;
    public Transform windowPoint;
    public Transform exitPoint;

    [Header("Desk Setup (Client Sockets)")]
    public XRSocketInteractor primarySocket;
    public XRSocketInteractor secondarySocket;

    [Header("Timing & Settings")]
    public float spawnDelay = 1.0f;

    private GameObject currentNPC;
    private bool isSpawning = false;

    public void SpawnNextNPC()
    {
        if (currentNPC != null)
        {
            Debug.Log("Υπάρχει ήδη κάποιος στο γκισέ! Πρέπει να φύγει πρώτα.");
            return;
        }

        if (isSpawning)
        {
            return;
        }

        StartCoroutine(SpawnWithDelayRoutine());
    }

    private IEnumerator SpawnWithDelayRoutine()
    {
        isSpawning = true;

        yield return new WaitForSeconds(spawnDelay);

        currentNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        NPCController controller = currentNPC.GetComponent<NPCController>();

        if (controller != null)
        {
            // --- ΑΝΤΛΗΣΗ ΠΙΘΑΝΟΤΗΤΩΝ ΑΠΟ ΤΟ DAY MANAGER ---
            float merSpawnProb = 0f;
            float mercSpawnProb = 0f;

            if (DayManager.Instance != null)
            {
                DaySettings today = DayManager.Instance.GetCurrentDaySettings();
                if (today != null)
                {
                    merSpawnProb = today.merchantSpawnProbability;
                    mercSpawnProb = today.mercenarySpawnProbability;
                }
            }

            // Ρίχνουμε το ζάρι από το 0 έως το 1
            float roll = Random.value;

            GameObject[] docsToSpawn;
            XRSocketInteractor[] socketsToUse;

            // ΕΛΕΓΧΟΣ 1: Είναι Έμπορος;
            if (roll < merSpawnProb && merchantPermitPrefab != null && secondarySocket != null)
            {
                docsToSpawn = new GameObject[] { passportPrefab, merchantPermitPrefab };
                socketsToUse = new XRSocketInteractor[] { primarySocket, secondarySocket };
                Debug.Log("<color=yellow>Έφτασε πελάτης: ΕΜΠΟΡΟΣ</color>");
            }
            // ΕΛΕΓΧΟΣ 2: Είναι Στρατιώτης;
            else if (roll < (merSpawnProb + mercSpawnProb) && mercenaryWritPrefab != null)
            {
                docsToSpawn = new GameObject[] { mercenaryWritPrefab };
                socketsToUse = new XRSocketInteractor[] { primarySocket };
                Debug.Log("<color=blue>Έφτασε πελάτης: ΣΤΡΑΤΙΩΤΗΣ</color>");
            }
            // ΕΛΕΓΧΟΣ 3: Απλός Πολίτης (Χωρικός/Πρόσφυγας)
            else
            {
                docsToSpawn = new GameObject[] { passportPrefab };
                socketsToUse = new XRSocketInteractor[] { primarySocket };
                Debug.Log("<color=white>Έφτασε πελάτης: ΑΠΛΟΣ ΠΟΛΙΤΗΣ</color>");
            }

            controller.Setup(spawnPoint, windowPoint, exitPoint, docsToSpawn, socketsToUse);
        }

        isSpawning = false;
    }

    // --- ΝΕΑ ΜΕΘΟΔΟΣ: Καθαρίζει τα πάντα για τη νέα μέρα ---
    public void ResetSpawner()
    {
        StopAllCoroutines();
        isSpawning = false;

        // Διαγραφή του NPC αν υπάρχει
        if (currentNPC != null)
        {
            Destroy(currentNPC);
            currentNPC = null;
        }

        // Σάρωση και διαγραφή όλων των εγγράφων που έχουν μείνει στη σκηνή
        DynamicPassport[] passports = FindObjectsOfType<DynamicPassport>();
        foreach (var p in passports) Destroy(p.gameObject);

        MerchantPermit[] permits = FindObjectsOfType<MerchantPermit>();
        foreach (var p in permits) Destroy(p.gameObject);

        MercenaryWrit[] writs = FindObjectsOfType<MercenaryWrit>();
        foreach (var w in writs) Destroy(w.gameObject);

        Debug.Log("Το γραφείο και οι NPCs καθαρίστηκαν επιτυχώς για τη νέα μέρα.");
    }
}