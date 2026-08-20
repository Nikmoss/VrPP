using UnityEngine;
using UnityEngine.InputSystem; // Απαραίτητο για τα κουμπιά του Controller

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
        // Ενεργοποιούμε την παρακολούθηση του κουμπιού
        if (resetButtonAction != null && resetButtonAction.action != null)
        {
            resetButtonAction.action.Enable();
            resetButtonAction.action.performed += OnResetButtonPressed;
        }
    }

    private void OnDisable()
    {
        // Σταματάμε να ακούμε το κουμπί όταν κλείνει το script (για αποφυγή errors)
        if (resetButtonAction != null && resetButtonAction.action != null)
        {
            resetButtonAction.action.performed -= OnResetButtonPressed;
            resetButtonAction.action.Disable();
        }
    }

    private void OnResetButtonPressed(InputAction.CallbackContext context)
    {
        if (dayAdvancer != null)
        {
            dayAdvancer.ResetAllTools();

            if (resetSound != null) resetSound.Play();

            Debug.Log("<color=cyan>Τα εργαλεία επέστρεψαν με το πάτημα του Controller!</color>");
        }
        else
        {
            Debug.LogWarning("Ξέχασες να βάλεις το CandleDayAdvancer στο ControllerToolReset!");
        }
    }
}