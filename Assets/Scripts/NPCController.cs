using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class NPCController : MonoBehaviour
{
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

        spawnedDocuments.Clear();
        for (int i = 0; i < documentPrefabs.Length; i++)
        {
            if (documentPrefabs[i] != null && clientSockets[i] != null)
            {
                GameObject doc = Instantiate(documentPrefabs[i], clientSockets[i].transform.position, clientSockets[i].transform.rotation);
                spawnedDocuments.Add(doc);
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

        // Βρίσκουμε τον ScoreManager στη σκηνή
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();

        if (scoreManager == null)
        {
            Debug.LogError("<color=red>ΣΦΑΛΜΑ:</color> Δεν μπόρεσα να βρω το ScoreManager! Σιγουρέψου ότι υπάρχει στη σκηνή και είναι ενεργοποιημένο.");
        }

        // Ελέγχουμε τι έγγραφα βρίσκονται στα Sockets
        foreach (var socket in clientSockets)
        {
            if (socket != null && socket.hasSelection)
            {
                GameObject item = socket.GetOldestInteractableSelected().transform.gameObject;

                if (item.GetComponent<DynamicPassport>() != null)
                    passport = item.GetComponent<DynamicPassport>();

                if (item.GetComponent<MerchantPermit>() != null)
                    permit = item.GetComponent<MerchantPermit>();
            }
        }

        // ΣΕΝΑΡΙΟ 1: Έμπορος (2 Χαρτιά)
        if (passport != null && permit != null)
        {
            // Για να είναι σωστός ο έμπορος πρέπει και τα ονόματα να ταιριάζουν ΚΑΙ να μην έχει λήξει
            bool namesMatch = (passport.currentFirstName == permit.currentFirstName) &&
                              (passport.currentLastName == permit.currentLastName);
            bool isNotExpired = !passport.isExpired;

            bool shouldBeApproved = namesMatch && isNotExpired;

            // Ελέγχουμε αν ο παίκτης έβαλε έγκριση και στα 2
            bool bothApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved &&
                                 permit.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved && bothApproved)
            {
                Debug.Log("<color=green>ΣΩΣΤΟ!</color> Ονόματα OK και έγκυρο. Εγκρίθηκαν και τα 2.");
                if (scoreManager != null) scoreManager.AddScore();
            }
            else if (!shouldBeApproved && !bothApproved)
            {
                Debug.Log("<color=green>ΣΩΣΤΟ!</color> Βρήκες το λάθος (ονόματα ή λήξη) και τα απέρριψες!");
                if (scoreManager != null) scoreManager.AddScore();
            }
            else
            {
                Debug.Log("<color=red>ΛΑΘΟΣ!</color> Η απόφασή σου δεν ήταν σωστή για τον έμπορο.");
                if (scoreManager != null) scoreManager.SubtractScore();
            }
        }
        // ΣΕΝΑΡΙΟ 2: Απλός Πολίτης (1 Χαρτί - Διαβατήριο)
        else if (passport != null)
        {
            bool shouldBeApproved = !passport.isExpired;
            bool isApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == isApproved)
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