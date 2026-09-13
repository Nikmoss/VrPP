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

        // --- 1. ΕΛΕΓΧΟΣ ΤΗΣ ΟΥΡΑΣ ΑΠΟ ΤΟ DAY MANAGER ---
        EncounterSetup nextEncounter = null;
        bool isQueueExhausted = false;

        if (DayManager.Instance != null)
        {
            DaySettings today = DayManager.Instance.GetCurrentDaySettings();

            // Ελέγχουμε αν υπάρχει στημένη ουρά για σήμερα
            if (today != null && today.dailyQueue != null && today.dailyQueue.Count > 0)
            {
                nextEncounter = DayManager.Instance.GetNextEncounter();

                if (nextEncounter == null)
                {
                    isQueueExhausted = true; // Τελείωσαν οι πελάτες της ημέρας
                }
            }
        }

        // Αν τελείωσε η ουρά, δεν κάνουμε spawn
        if (isQueueExhausted)
        {
            Debug.Log("<color=green>Τέλος Βάρδιας!</color> Δεν υπάρχουν άλλοι πελάτες στην ουρά.");
            isSpawning = false;
            yield break;
        }

        // --- 2. ΔΗΜΙΟΥΡΓΙΑ NPC ---
        currentNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);
        NPCController controller = currentNPC.GetComponent<NPCController>();

        if (controller != null)
        {
            GameObject[] docsToSpawn = null;
            XRSocketInteractor[] socketsToUse = null;

            // --- 3. ΑΠΟΦΑΣΗ ΤΥΠΟΥ ΠΕΛΑΤΗ (Με βάση την ουρά ή τυχαία αν δεν υπάρχει) ---
            if (nextEncounter != null)
            {
                // ΠΑΙΖΟΥΜΕ ΜΕ ΤΗΝ ΟΥΡΑ (Scripted ή Random από το Inspector)
                if (nextEncounter.npcType == NPCType.ScriptedMerchant || nextEncounter.npcType == NPCType.RandomMerchant)
                {
                    docsToSpawn = new GameObject[] { passportPrefab, merchantPermitPrefab };
                    socketsToUse = new XRSocketInteractor[] { primarySocket, secondarySocket };
                    Debug.Log($"<color=yellow>Έφτασε πελάτης (Από Ουρά): {nextEncounter.npcType} - [{nextEncounter.inspectorNote}]</color>");
                }
                else // Citizen
                {
                    docsToSpawn = new GameObject[] { passportPrefab };
                    socketsToUse = new XRSocketInteractor[] { primarySocket };
                    Debug.Log($"<color=white>Έφτασε πελάτης (Από Ουρά): {nextEncounter.npcType} - [{nextEncounter.inspectorNote}]</color>");
                }
            }
            else
            {
                // ΠΑΙΖΟΥΜΕ ΜΕ ΤΟ ΠΑΛΙΟ RANDOM ΣΥΣΤΗΜΑ (Αν η ουρά είναι άδεια, π.χ. Day 1, 2, 3)
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

                float roll = Random.value;

                if (roll < merSpawnProb && merchantPermitPrefab != null && secondarySocket != null)
                {
                    docsToSpawn = new GameObject[] { passportPrefab, merchantPermitPrefab };
                    socketsToUse = new XRSocketInteractor[] { primarySocket, secondarySocket };
                    Debug.Log("<color=yellow>Έφτασε πελάτης (Τυχαία): ΕΜΠΟΡΟΣ</color>");
                }
                else if (roll < (merSpawnProb + mercSpawnProb) && mercenaryWritPrefab != null)
                {
                    docsToSpawn = new GameObject[] { mercenaryWritPrefab };
                    socketsToUse = new XRSocketInteractor[] { primarySocket };
                    Debug.Log("<color=blue>Έφτασε πελάτης (Τυχαία): ΣΤΡΑΤΙΩΤΗΣ</color>");
                }
                else
                {
                    docsToSpawn = new GameObject[] { passportPrefab };
                    socketsToUse = new XRSocketInteractor[] { primarySocket };
                    Debug.Log("<color=white>Έφτασε πελάτης (Τυχαία): ΑΠΛΟΣ ΠΟΛΙΤΗΣ</color>");
                }
            }

            // Στήσιμο εγγράφω
            controller.Setup(spawnPoint, windowPoint, exitPoint, docsToSpawn, socketsToUse, nextEncounter);
        }

        isSpawning = false;
    }

    public void ResetSpawner()
    {
        StopAllCoroutines();
        isSpawning = false;

        if (currentNPC != null)
        {
            Destroy(currentNPC);
            currentNPC = null;
        }

        DynamicPassport[] passports = FindObjectsOfType<DynamicPassport>();
        foreach (var p in passports) Destroy(p.gameObject);

        MerchantPermit[] permits = FindObjectsOfType<MerchantPermit>();
        foreach (var p in permits) Destroy(p.gameObject);

        MercenaryWrit[] writs = FindObjectsOfType<MercenaryWrit>();
        foreach (var w in writs) Destroy(w.gameObject);

        Debug.Log("Το γραφείο και οι NPCs καθαρίστηκαν επιτυχώς για τη νέα μέρα.");
    }
}