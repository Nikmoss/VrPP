using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class NPCController : MonoBehaviour
{
    [Header("Ρυθμίσεις Δυσκολίας")]
    [Range(0f, 1f)]
    [Tooltip("Η πιθανότητα (0.0 έως 1.0) τα ονόματα του εμπόρου να ΔΕΝ ταιριάζουν.")]
    public float mismatchProbability = 0.3f;

    private Transform spawnPoint;
    private Transform windowPoint;
    private Transform exitPoint;

    private GameObject[] documentPrefabs;
    private XRSocketInteractor[] clientSockets;
    private float moveSpeed = 1.5f;

    private List<GameObject> spawnedDocuments = new List<GameObject>();
    private bool isArrested = false;

    public void Setup(Transform spawn, Transform window, Transform exit, GameObject[] docs, XRSocketInteractor[] sockets)
    {
        spawnPoint = spawn;
        windowPoint = window;
        exitPoint = exit;
        documentPrefabs = docs;
        clientSockets = sockets;

        StartCoroutine(NPCFlowRoutine());
    }

    private IEnumerator NPCFlowRoutine()
    {
        transform.position = spawnPoint.position;
        while (Vector3.Distance(transform.position, windowPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, windowPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 1. Γεννάμε τα χαρτιά
        spawnedDocuments.Clear();
        DynamicPassport spawnedPassport = null;
        MerchantPermit spawnedPermit = null;

        for (int i = 0; i < documentPrefabs.Length; i++)
        {
            if (documentPrefabs[i] != null && clientSockets[i] != null)
            {
                GameObject doc = Instantiate(documentPrefabs[i], clientSockets[i].transform.position, clientSockets[i].transform.rotation);
                spawnedDocuments.Add(doc);

                if (doc.GetComponent<DynamicPassport>() != null) spawnedPassport = doc.GetComponent<DynamicPassport>();
                if (doc.GetComponent<MerchantPermit>() != null) spawnedPermit = doc.GetComponent<MerchantPermit>();
            }
        }

        // 2. Λογική Πιθανοτήτων: Ελέγχουμε αν τα ονόματα θα ταιριάζουν ή όχι
        if (spawnedPassport != null && spawnedPermit != null)
        {
            bool shouldMismatch = Random.value < mismatchProbability;

            if (!shouldMismatch)
            {
                // Επιβάλλουμε τα ονόματα να είναι ΙΔΙΑ με του διαβατηρίου (Κανονικός Έμπορος)
                spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
            }
            else
            {
                // Επιβάλλουμε να είναι ΔΙΑΦΟΡΕΤΙΚΑ (Πλαστογράφος)
                while (spawnedPermit.currentFirstName == spawnedPassport.currentFirstName &&
                       spawnedPermit.currentLastName == spawnedPassport.currentLastName)
                {
                    spawnedPermit.GenerateData(); // Ξαναδιαλέγει τυχαία μέχρι να μην ταιριάζουν
                }
            }
        }

        bool readyToLeave = false;
        while (!readyToLeave)
        {
            if (isArrested) yield break;

            int stampedCount = 0;
            int totalRequired = documentPrefabs.Length;

            for (int i = 0; i < totalRequired; i++)
            {
                if (clientSockets[i] != null && clientSockets[i].hasSelection)
                {
                    GameObject itemInSocket = clientSockets[i].GetOldestInteractableSelected().transform.gameObject;

                    if (IsDocumentStamped(itemInSocket))
                    {
                        stampedCount++;
                    }
                }
            }

            if (stampedCount >= totalRequired && totalRequired > 0)
            {
                readyToLeave = true;
            }

            yield return null;
        }

        yield return new WaitForSeconds(1.0f);

        // --- ΑΞΙΟΛΟΓΗΣΗ ΚΑΙ ΠΟΝΤΟΙ ---
        EvaluatePlayerDecision();

        foreach (var doc in spawnedDocuments)
        {
            if (doc != null) Destroy(doc);
        }

        while (Vector3.Distance(transform.position, exitPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(gameObject);
    }

    private void EvaluatePlayerDecision()
    {
        DynamicPassport passport = null;
        MerchantPermit permit = null;
        MercenaryWrit writ = null; // ΝΕΟ: Αναφορά στο χαρτί του Στρατιώτη

        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogError("<color=red>ΣΦΑΛΜΑ:</color> Δεν μπόρεσα να βρω το ScoreManager!");
        }

        foreach (var socket in clientSockets)
        {
            if (socket != null && socket.hasSelection)
            {
                GameObject item = socket.GetOldestInteractableSelected().transform.gameObject;

                if (item.GetComponent<DynamicPassport>() != null)
                    passport = item.GetComponent<DynamicPassport>();

                if (item.GetComponent<MerchantPermit>() != null)
                    permit = item.GetComponent<MerchantPermit>();

                if (item.GetComponent<MercenaryWrit>() != null)
                    writ = item.GetComponent<MercenaryWrit>(); // ΝΕΟ: Εντοπισμός
            }
        }

        // --- ΣΕΝΑΡΙΟ 1: Έμπορος (2 Χαρτιά) ---
        if (passport != null && permit != null)
        {
            bool namesMatch = (passport.currentFirstName == permit.currentFirstName) &&
                              (passport.currentLastName == permit.currentLastName);
            bool isNotExpired = !passport.isExpired;

            bool shouldBeApproved = namesMatch && isNotExpired;
            bool playerApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved &&
                                   permit.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved && playerApproved)
            {
                Debug.Log("<color=green>ΣΩΣΤΟ!</color> Ονόματα ίδια ΚΑΙ έγκυρο. Το ενέκρινες.");
                if (scoreManager != null) scoreManager.AddScore();
            }
            else if (!shouldBeApproved && !playerApproved)
            {
                if (!namesMatch)
                    Debug.Log("<color=green>ΣΩΣΤΟ!</color> Βρήκες τα διαφορετικά ονόματα και το απέρριψες!");
                else
                    Debug.Log("<color=green>ΣΩΣΤΟ!</color> Ίδια ονόματα, ΑΛΛΑ ήταν ληγμένο και το απέρριψες!");

                if (scoreManager != null) scoreManager.AddScore();
            }
            else
            {
                Debug.Log("<color=red>ΛΑΘΟΣ!</color> Η απόφασή σου ήταν λανθασμένη για τον Έμπορο.");
                if (scoreManager != null) scoreManager.SubtractScore();
            }
        }
        // --- ΣΕΝΑΡΙΟ 2: Στρατιώτης (1 Χαρτί - Mercenary Writ) ---
        else if (writ != null)
        {
            bool shouldBeApproved = !writ.isForged; // Αν δεν είναι πλαστό, πρέπει να εγκριθεί
            bool playerApproved = (writ.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == playerApproved)
            {
                Debug.Log("<color=green>ΣΩΣΤΟ!</color> Σωστή απόφαση για τον Στρατιώτη.");
                if (scoreManager != null) scoreManager.AddScore();
            }
            else
            {
                Debug.Log("<color=red>ΛΑΘΟΣ!</color> Λάθος απόφαση για τον Στρατιώτη.");
                if (scoreManager != null) scoreManager.SubtractScore();
            }
        }
        // --- ΣΕΝΑΡΙΟ 3: Απλός Πολίτης (1 Χαρτί - Διαβατήριο) ---
        else if (passport != null)
        {
            bool shouldBeApproved = !passport.isExpired;
            bool playerApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == playerApproved)
            {
                Debug.Log("<color=green>ΣΩΣΤΟ!</color> Σωστή απόφαση για το διαβατήριο.");
                if (scoreManager != null) scoreManager.AddScore();
            }
            else
            {
                Debug.Log("<color=red>ΛΑΘΟΣ!</color> Λάθος απόφαση για το διαβατήριο.");
                if (scoreManager != null) scoreManager.SubtractScore();
            }
        }
    }

    private bool IsDocumentStamped(GameObject item)
    {
        DynamicPassport passport = item.GetComponent<DynamicPassport>();
        if (passport != null) return passport.hasBeenStamped;

        MerchantPermit permit = item.GetComponent<MerchantPermit>();
        if (permit != null) return permit.hasBeenStamped;

        // ΝΕΟ: Έλεγχος αν το στρατιωτικό χαρτί έχει σφραγιστεί/υπογραφεί
        MercenaryWrit writ = item.GetComponent<MercenaryWrit>();
        if (writ != null) return writ.hasBeenStamped;

        return false;
    }

    public void DocumentWasStamped() { }

    public void ArrestNPC()
    {
        isArrested = true;
        StopAllCoroutines();

        foreach (var doc in spawnedDocuments)
        {
            if (doc != null) Destroy(doc);
        }
        Destroy(gameObject, 1.5f);
    }
}