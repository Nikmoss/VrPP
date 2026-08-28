using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerToolReset : MonoBehaviour
{
    [Header("Αναφορές")]
    [Tooltip("Το αντικείμενο του Κεριού που περιέχει το script CandleDayAdvancer")]
    public CandleDayAdvancer dayAdvancer;

    [Header("Είσοδος (Input)")]
    [Tooltip("Επίλεξε εδώ ποιο κουμπί θέλεις να κάνει το Reset (π.χ. LeftHand/PrimaryButton)")]
    public InputActionProperty resetButtonAction;

    [Header("Ήχος (Προαιρετικό)")]
    public AudioSource resetSound;

    private void OnEnable()
    {
        if (resetButtonAction != null && resetButtonAction.action != null)
        {
            resetButtonAction.action.Enable();
            resetButtonAction.action.performed += OnResetButtonPressed;
        }
    }

    private void OnDisable()
    {
        if (resetButtonAction != null && resetButtonAction.action != null)
        {
            resetButtonAction.action.performed -= OnResetButtonPressed;
            resetButtonAction.action.Disable();
        }
    }

    private void OnResetButtonPressed(InputAction.CallbackContext context)
    {
        bool didResetSomething = false;

        // 1. Επαναφορά των Εργαλείων (Σφραγίδες, κτλ.)
        if (dayAdvancer != null)
        {
            dayAdvancer.ResetAllTools();
            didResetSomething = true;
        }
        else
        {
            Debug.LogWarning("Ξέχασες να βάλεις το CandleDayAdvancer στο ControllerToolReset!");
        }

        // 2. Επαναφορά των Χαρτιών του ενεργού NPC
        NPCController activeNPC = FindObjectOfType<NPCController>();
        if (activeNPC != null)
        {
            activeNPC.ResetDocumentsToSockets();
            didResetSomething = true;
            Debug.Log("<color=cyan>Τα έγγραφα του NPC επέστρεψαν στο γραφείο!</color>");
        }

        // Παίζουμε τον ήχο μόνο αν όντως έγινε επαναφορά (είτε εργαλείων είτε χαρτιών)
        if (didResetSomething && resetSound != null)
        {
            resetSound.Play();
        }
    }
}