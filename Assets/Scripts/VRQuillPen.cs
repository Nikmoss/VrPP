using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRQuillPen : MonoBehaviour
{
    [Header("Setup")]
    public Transform tipPoint;
    public Material inkMaterial;
    public AudioSource scratchSound;

    [Header("Ink Settings")]
    public float inkWidth = 0.01f;
    public float minSpacing = 0.005f;
    public float zFightingOffset = 0.001f;

    [Header("Gameplay Logic")]
    public int inkPointsToReject = 15;
    public VelocityStampTool.StampDecision penDecision;

    private XRGrabInteractable grabInteractable;
    private LineRenderer currentLine;
    private Transform currentPaper;
    private Collider currentPaperCollider;
    private bool isDrawing = false;
    private Vector3 lastLocalPoint;

    private int totalInkPointsOnCurrentPaper = 0;
    private bool paperHasBeenRejected = false;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Document") && grabInteractable != null && grabInteractable.isSelected)
        {
            if (currentPaper != other.transform)
            {
                totalInkPointsOnCurrentPaper = 0;
                paperHasBeenRejected = false;
            }

            currentPaper = other.transform;
            currentPaperCollider = other;
            StartDrawing();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Document"))
        {
            StopDrawing();
        }
    }

    private void StartDrawing()
    {
        if (isDrawing) return;
        isDrawing = true;

        GameObject lineObj = new GameObject("InkLine");
        lineObj.transform.SetParent(currentPaper);
        lineObj.transform.localPosition = Vector3.zero;
        lineObj.transform.localRotation = Quaternion.identity;
        lineObj.transform.localScale = Vector3.one;

        currentLine = lineObj.AddComponent<LineRenderer>();
        currentLine.material = inkMaterial;
        currentLine.useWorldSpace = false;

        currentLine.startWidth = inkWidth;
        currentLine.endWidth = inkWidth;
        currentLine.positionCount = 0;

        lastLocalPoint = new Vector3(999f, 999f, 999f);

        if (scratchSound != null) scratchSound.Play();
    }

    private void Update()
    {
        if (!isDrawing) return;

        if (grabInteractable != null && !grabInteractable.isSelected)
        {
            StopDrawing();
            return;
        }

        if (currentLine != null && currentPaper != null && currentPaperCollider != null)
        {
            Vector3 surfacePoint = currentPaperCollider.ClosestPoint(tipPoint.position);
            Vector3 raisedPoint = surfacePoint + (currentPaper.up * zFightingOffset);
            Vector3 localPos = currentPaper.InverseTransformPoint(raisedPoint);

            if (Vector3.Distance(localPos, lastLocalPoint) > minSpacing)
            {
                currentLine.positionCount++;
                currentLine.SetPosition(currentLine.positionCount - 1, localPos);
                lastLocalPoint = localPos;

                totalInkPointsOnCurrentPaper++;

                if (totalInkPointsOnCurrentPaper > inkPointsToReject && !paperHasBeenRejected)
                {
                    RejectDocumentLogic();
                }
            }
        }
    }

    private void StopDrawing()
    {
        isDrawing = false;
        currentLine = null;
        if (scratchSound != null) scratchSound.Stop();
    }

    private void RejectDocumentLogic()
    {
        paperHasBeenRejected = true;
        Debug.Log("Το έγγραφο σημειώθηκε με την πένα! ΑΠΟΡΡΙΠΤΕΤΑΙ.");

        bool documentFound = false;

        // --- 1. Έλεγχος για Διαβατήριο ---
        DynamicPassport passport = currentPaper.GetComponentInParent<DynamicPassport>();
        if (passport == null)
        {
            passport = currentPaper.GetComponentInChildren<DynamicPassport>();
        }

        if (passport != null)
        {
            passport.hasBeenStamped = true;
            passport.lastAppliedStamp = penDecision;
            Debug.Log($"<color=orange>Η Πένα βρήκε το διαβατήριο και έστειλε απόφαση: {penDecision}</color>");
            documentFound = true;
        }

        // --- 2. Έλεγχος για Άδεια Εμπόρου ---
        MerchantPermit permit = currentPaper.GetComponentInParent<MerchantPermit>();
        if (permit == null)
        {
            permit = currentPaper.GetComponentInChildren<MerchantPermit>();
        }

        if (permit != null)
        {
            permit.hasBeenStamped = true;
            permit.lastAppliedStamp = penDecision;
            Debug.Log($"<color=orange>Η Πένα βρήκε την Άδεια Εμπόρου και έστειλε απόφαση: {penDecision}</color>");
            documentFound = true;
        }

        // Αν δεν βρήκε κανένα από τα δύο
        if (!documentFound)
        {
            Debug.LogError("<color=red>ΣΦΑΛΜΑ: Η πένα ζωγράφισε, αλλά δεν μπόρεσε να βρει ούτε Διαβατήριο ούτε Άδεια στο χαρτί!</color>");
        }

        NPCController currentNPC = FindObjectOfType<NPCController>();
        if (currentNPC != null)
        {
            currentNPC.DocumentWasStamped();
        }
    }
}