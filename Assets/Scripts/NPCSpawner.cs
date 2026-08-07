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
    public GameObject mercenaryWritPrefab; // ΝΕΟ: Το Prefab του Στρατιώτη

    [Header("Waypoints")]
    public Transform spawnPoint;
    public Transform windowPoint;
    public Transform exitPoint;

    [Header("Desk Setup (Client Sockets)")]
    public XRSocketInteractor primarySocket;
    public XRSocketInteractor secondarySocket;

    [Header("Timing & Settings")]
    public float spawnDelay = 1.0f;

    [Tooltip("Πιθανότητα να εμφανιστεί Έμπορος (π.χ. 0.3 = 30%)")]
    [Range(0f, 1f)] public float merchantProbability = 0.3f;

    [Tooltip("Πιθανότητα να εμφανιστεί Στρατιώτης (π.χ. 0.3 = 30%)")]
    [Range(0f, 1f)] public float mercenaryProbability = 0.3f;
    // Αν δεν είναι ούτε το ένα ούτε το άλλο, έρχεται Απλός Πολίτης!

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
            // Ρίχνουμε το ζάρι από το 0 έως το 1
            float roll = Random.value;

            GameObject[] docsToSpawn;
            XRSocketInteractor[] socketsToUse;

            // ΕΛΕΓΧΟΣ 1: Είναι Έμπορος;
            if (roll < merchantProbability && merchantPermitPrefab != null && secondarySocket != null)
            {
                docsToSpawn = new GameObject[] { passportPrefab, merchantPermitPrefab };
                socketsToUse = new XRSocketInteractor[] { primarySocket, secondarySocket };
                Debug.Log("<color=yellow>Έφτασε πελάτης: ΕΜΠΟΡΟΣ</color>");
            }
            // ΕΛΕΓΧΟΣ 2: Είναι Στρατιώτης;
            else if (roll < (merchantProbability + mercenaryProbability) && mercenaryWritPrefab != null)
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
}