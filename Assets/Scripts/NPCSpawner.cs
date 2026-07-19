using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Το εργοστάσιο παραγωγής NPC. Μόλις φύγει ο ένας, στέλνει τον επόμενο.
/// </summary>
public class NPCSpawner : MonoBehaviour
{
    [Header("Ρυθμίσεις Spawner")]
    public NPCController npcPrefab; // Το φασόλι από τα Assets
    public GameObject passportPrefab; // Το διαβατήριο από τα Assets

    [Header("Σημεία Σκηνής")]
    public Transform spawnPoint;
    public Transform windowPoint;
    public Transform exitPoint;
    public XRSocketInteractor deskSocket; // Το Passport_Socket

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            // 1. Γεννάμε έναν νέο NPC
            NPCController newNPC = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);

            // 2. Του δίνουμε τις οδηγίες του (σημεία και αντικείμενα)
            newNPC.Setup(spawnPoint, windowPoint, exitPoint, passportPrefab, deskSocket);

            // 3. Περιμένουμε όσο αυτός ο NPC υπάρχει στη σκηνή (μέχρι να καταστραφεί)
            while (newNPC != null)
            {
                yield return null;
            }

            // 4. Ο NPC έφυγε. Περιμένουμε 3 δευτερόλεπτα (ανάσα) και στέλνουμε τον επόμενο!
            yield return new WaitForSeconds(3f);
        }
    }
}