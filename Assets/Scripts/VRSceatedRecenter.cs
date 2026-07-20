using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

/// <summary>
/// Κεντράρει τον παίκτη στην ιδανική θέση (καρέκλα) όταν πατάει ένα κουμπί.
/// Διατηρεί την άνεση του VR μετακινώντας το δωμάτιο αντί για την κάμερα.
/// </summary>
public class VRSeatedRecenter : MonoBehaviour
{
    [Header("Ρυθμίσεις Recenter")]
    [Tooltip("Το κουμπί για το Recenter (π.χ. Right Thumbstick Press)")]
    public InputActionProperty recenterAction;

    [Tooltip("Το ιδανικό σημείο που πρέπει να βρίσκεται το κεφάλι (άδειο GameObject πάνω από την καρέκλα)")]
    public Transform targetHeadPosition;

    private XROrigin xrOrigin;

    private void Awake()
    {
        xrOrigin = GetComponent<XROrigin>();
    }

    private void Start()
    {
        // Αυτόματο κεντράρισμα με το που ξεκινάει το παιχνίδι (δίνουμε 0.5s στο Headset να βρει τη θέση του)
        Invoke(nameof(Recenter), 0.5f);
    }

    private void OnEnable()
    {
        if (recenterAction.action != null)
        {
            recenterAction.action.Enable();
            recenterAction.action.performed += DoRecenter;
        }
    }

    private void OnDisable()
    {
        if (recenterAction.action != null)
        {
            recenterAction.action.performed -= DoRecenter;
            recenterAction.action.Disable();
        }
    }

    private void DoRecenter(InputAction.CallbackContext context)
    {
        Recenter();
    }

    public void Recenter()
    {
        if (xrOrigin == null || targetHeadPosition == null) return;

        // 1. Ευθυγράμμιση Περιστροφής: Στρίβει το δωμάτιο ώστε ο παίκτης να κοιτάει προς τα εκεί που κοιτάει το Target
        xrOrigin.MatchOriginUpCameraForward(targetHeadPosition.up, targetHeadPosition.forward);

        // 2. Ευθυγράμμιση Θέσης: Φέρνει το γραφείο ακριβώς κάτω από το πραγματικό κεφάλι του παίκτη
        xrOrigin.MoveCameraToWorldLocation(targetHeadPosition.position);

        Debug.Log("<color=cyan>VR Recenter:</color> Το δωμάτιο προσαρμόστηκε τέλεια στο ύψος/θέση του παίκτη!");
    }
}