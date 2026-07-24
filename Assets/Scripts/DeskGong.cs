using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DeskGong : MonoBehaviour
{
    [Header("Gong Settings")]
    public AudioSource gongSound;
    public NPCSpawner spawner;
    public float cooldown = 1.0f; // Πόσα δευτερόλεπτα πρέπει να περάσουν για να ξαναχτυπήσει

    private float lastHitTime = 0f;

    private void OnTriggerEnter(Collider other)
    {
        // Αποτροπή spamming (να μην χτυπάει πολλές φορές μαζεμένες)
        if (Time.time - lastHitTime < cooldown) return;

        // Εκτύπωση στην κονσόλα για να δούμε ακριβώς τι ακούμπησε το γκονγκ
        Debug.Log("Το γκονγκ ακουμπήθηκε από: " + other.gameObject.name);

        // Ελέγχουμε αν αυτό που μας ακούμπησε είναι το χέρι (Interactor) ή εργαλείο (Grab Interactable)
        bool isHand = other.GetComponentInParent<XRBaseInteractor>() != null;
        bool isTool = other.GetComponentInParent<XRGrabInteractable>() != null;

        if (isHand || isTool)
        {
            lastHitTime = Time.time;

            if (gongSound != null)
            {
                gongSound.Play();
            }

            if (spawner != null)
            {
                // Ειδοποιούμε τον Spawner να φέρει τον επόμενο
                spawner.SpawnNextNPC();
            }
        }
    }
}