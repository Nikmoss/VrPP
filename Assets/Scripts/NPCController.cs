using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Διαχειρίζεται το "Φασόλι". Περιμένει το χτύπημα ΚΑΙ την επιστροφή του χαρτιού στο γραφείο.
/// Στο τέλος υπολογίζει το σκορ βάσει της τελευταίας σφραγίδας που μπήκε.
/// </summary>
public class NPCController : MonoBehaviour
{
    private Transform spawnPoint;
    private Transform windowPoint;
    private Transform exitPoint;
    private GameObject passportPrefab;
    private XRSocketInteractor deskSocket;
    private float moveSpeed = 1.5f;

    private GameObject currentPassport;
    private bool isStamped = false;

    public void Setup(Transform spawn, Transform window, Transform exit, GameObject passport, XRSocketInteractor socket)
    {
        spawnPoint = spawn;
        windowPoint = window;
        exitPoint = exit;
        passportPrefab = passport;
        deskSocket = socket;

        StartCoroutine(NPCFlowRoutine());
    }

    private IEnumerator NPCFlowRoutine()
    {
        // 1. ΠΕΡΠΑΤΗΜΑ ΠΡΟΣ ΤΟ ΓΚΙΣΕ
        transform.position = spawnPoint.position;
        while (Vector3.Distance(transform.position, windowPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, windowPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 2. ΕΜΦΑΝΙΖΕΙ ΤΟ ΔΙΑΒΑΤΗΡΙΟ
        if (passportPrefab != null && deskSocket != null)
        {
            currentPassport = Instantiate(passportPrefab, deskSocket.transform.position, deskSocket.transform.rotation);
        }

        // 3. ΠΕΡΙΜΕΝΕΙ ΓΙΑ ΤΗ ΣΦΡΑΓΙΔΑ ***ΚΑΙ*** ΝΑ ΜΠΕΙ ΞΑΝΑ ΣΤΟ SOCKET
        while (!isStamped || !deskSocket.hasSelection)
        {
            yield return null;
        }

        // 4. ΟΚ! Το χαρτί σφραγίστηκε και αφέθηκε στο γραφείο. 
        yield return new WaitForSeconds(1.0f);

        // 5. ΠΑΙΡΝΕΙ ΤΟ ΔΙΑΒΑΤΗΡΙΟ ΚΑΙ ΥΠΟΛΟΓΙΖΕΤΑΙ ΤΟ ΣΚΟΡ ΜΙΑ ΦΟΡΑ!
        if (currentPassport != null)
        {
            DynamicPassport passportData = currentPassport.GetComponent<DynamicPassport>();
            if (passportData != null && passportData.hasBeenStamped)
            {
                ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
                if (scoreManager != null)
                {
                    // Περνάμε την ΤΕΛΕΥΤΑΙΑ απόφαση που σώθηκε στο χαρτί
                    scoreManager.EvaluateDecision(passportData.lastAppliedStamp, passportData);
                }
            }

            Destroy(currentPassport);
        }

        // 6. ΦΕΥΓΕΙ ΠΡΟΣ ΤΗΝ ΕΞΟΔΟ
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 7. ΕΞΑΦΑΝΙΖΕΤΑΙ
        Destroy(gameObject);
    }

    public void DocumentWasStamped()
    {
        isStamped = true;
    }
}