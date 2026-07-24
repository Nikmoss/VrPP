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

    private GameObject currentNPC;

    // Αυτή η μέθοδος πλέον καλείται ΜΟΝΟ όταν ο παίκτης χτυπάει το γκονγκ
    public void SpawnNextNPC()
    {
        // Ελέγχουμε αν υπάρχει ήδη πελάτης στο γκισέ
        if (currentNPC != null)
        {
            Debug.Log("Υπάρχει ήδη κάποιος στο γκισέ! Πρέπει να φύγει πρώτα.");
            return;
        }

        // Δημιουργία του νέου NPC
        currentNPC = Instantiate(npcPrefab, spawnPoint.position, spawnPoint.rotation);

        NPCController controller = currentNPC.GetComponent<NPCController>();
        if (controller != null)
        {
            controller.Setup(spawnPoint, windowPoint, exitPoint, passportPrefab, deskSocket);
        }
    }
}