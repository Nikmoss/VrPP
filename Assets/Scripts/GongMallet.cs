using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GongMallet : MonoBehaviour
{
    [Header("Haptic Settings")]
    [Range(0f, 1f)] public float hitAmplitude = 0.8f;
    public float hitDuration = 0.15f;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        // Βρίσκουμε το component που μας επιτρέπει να πιάνουμε το ραβδάκι
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ελέγχουμε αν αυτό που χτυπήσαμε είναι το γκονγκ ΚΑΙ αν κρατάμε το ραβδάκι
        if (other.gameObject.name.Contains("Gong") && grabInteractable != null && grabInteractable.isSelected)
        {
            TriggerHaptics();
        }
    }

    private void TriggerHaptics()
    {
        // Βρίσκουμε ποιο χέρι κρατάει το ραβδάκι και στέλνουμε δόνηση
        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            HapticImpulsePlayer hapticPlayer = interactor.transform.GetComponentInParent<HapticImpulsePlayer>();
            if (hapticPlayer == null)
            {
                hapticPlayer = interactor.transform.GetComponentInChildren<HapticImpulsePlayer>();
            }

            if (hapticPlayer != null)
            {
                hapticPlayer.SendHapticImpulse(hitAmplitude, hitDuration);
            }
        }
    }
}