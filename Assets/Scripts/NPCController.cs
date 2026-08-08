using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class NPCController : MonoBehaviour
{
    [Header("Ρυθμίσεις Δυσκολίας - Meters Πλαστογραφίας")]
    [Range(0f, 1f)]
    public float citizenForgeryProbability = 0.3f;

    [Range(0f, 1f)]
    public float merchantForgeryProbability = 0.3f;

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
        spawnPoint = spawn; windowPoint = window; exitPoint = exit;
        documentPrefabs = docs; clientSockets = sockets;
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
        DynamicPassport spawnedPassport = null;
        MerchantPermit spawnedPermit = null;
        MercenaryWrit spawnedWrit = null;

        for (int i = 0; i < documentPrefabs.Length; i++)
        {
            if (documentPrefabs[i] != null && clientSockets[i] != null)
            {
                GameObject doc = Instantiate(documentPrefabs[i], clientSockets[i].transform.position, clientSockets[i].transform.rotation);
                spawnedDocuments.Add(doc);
                if (doc.GetComponent<DynamicPassport>() != null) spawnedPassport = doc.GetComponent<DynamicPassport>();
                if (doc.GetComponent<MerchantPermit>() != null) spawnedPermit = doc.GetComponent<MerchantPermit>();
                if (doc.GetComponent<MercenaryWrit>() != null) spawnedWrit = doc.GetComponent<MercenaryWrit>();
            }
        }

        // --- ΛΟΓΙΚΗ ΓΕΝΝΗΣΗΣ ΧΑΡΤΙΩΝ ΒΑΣΕΙ ΤΩΝ METERS ---
        if (spawnedPassport != null && spawnedPermit != null)
        {
            bool isMerchantForged = Random.value < merchantForgeryProbability;

            if (!isMerchantForged)
            {
                // ΣΩΣΤΟΣ ΕΜΠΟΡΟΣ
                spawnedPassport.GenerateData(false);
                spawnedPassport.currentPurpose = "Trade";

                spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                spawnedPermit.currentCity = spawnedPassport.originCityName;

                spawnedPermit.issueDay = spawnedPassport.issueDay;
                spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                spawnedPermit.issueYear = spawnedPassport.issueYear;
                if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
            }
            else
            {
                // ΠΛΑΣΤΟΓΡΑΦΟΣ ΕΜΠΟΡΟΣ: Μπορεί το λάθος να είναι το Διαβατήριο (0) ή η Σύγκριση (1-4)
                int errorType = Random.Range(0, 5);

                if (errorType == 0)
                {
                    // ΝΕΟ: Το Διαβατήριο ΕΙΝΑΙ το πρόβλημα (Ληγμένο ή Λάθος Έμβλημα)!
                    spawnedPassport.GenerateData(true);
                    spawnedPassport.currentPurpose = "Trade"; // Το κρατάμε Trade για να είναι μόνο ΕΝΑ το λάθος

                    // Το Permit ταιριάζει απόλυτα με ό,τι λέει το Passport
                    spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                    spawnedPermit.currentCity = spawnedPassport.originCityName;
                    spawnedPermit.issueDay = spawnedPassport.issueDay;
                    spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                    spawnedPermit.issueYear = spawnedPassport.issueYear;
                    if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                }
                else if (errorType == 1)
                {
                    // Λάθος Όνομα (Απλή μαθηματική μετατόπιση λίστας, κανένα κόλλημα)
                    spawnedPassport.GenerateData(false);
                    spawnedPassport.currentPurpose = "Trade";

                    int wrongFirstIndex = (System.Array.IndexOf(spawnedPermit.firstNames, spawnedPassport.currentFirstName) + 1) % spawnedPermit.firstNames.Length;
                    int wrongLastIndex = (System.Array.IndexOf(spawnedPermit.lastNames, spawnedPassport.currentLastName) + 1) % spawnedPermit.lastNames.Length;

                    spawnedPermit.currentFirstName = spawnedPermit.firstNames[wrongFirstIndex];
                    spawnedPermit.currentLastName = spawnedPermit.lastNames[wrongLastIndex];

                    spawnedPermit.currentCity = spawnedPassport.originCityName;
                    spawnedPermit.issueDay = spawnedPassport.issueDay;
                    spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                    spawnedPermit.issueYear = spawnedPassport.issueYear;
                    if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                }
                else if (errorType == 2)
                {
                    // Λάθος Πόλη 
                    spawnedPassport.GenerateData(false);
                    spawnedPassport.currentPurpose = "Trade";
                    spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);

                    int wrongCityIndex = (System.Array.IndexOf(spawnedPermit.cities, spawnedPassport.originCityName) + 1) % spawnedPermit.cities.Length;
                    spawnedPermit.currentCity = spawnedPermit.cities[wrongCityIndex];

                    spawnedPermit.issueDay = spawnedPassport.issueDay;
                    spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                    spawnedPermit.issueYear = spawnedPassport.issueYear;
                    if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                }
                else if (errorType == 3)
                {
                    // Λάθος Ημερομηνία 
                    spawnedPassport.GenerateData(false);
                    spawnedPassport.currentPurpose = "Trade";
                    spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                    spawnedPermit.currentCity = spawnedPassport.originCityName;

                    spawnedPermit.issueDay = spawnedPassport.issueDay;
                    spawnedPermit.issueYear = spawnedPassport.issueYear;
                    spawnedPermit.issueMonth = spawnedPassport.issueMonth - Random.Range(1, 4);

                    if (spawnedPermit.issueMonth <= 0)
                    {
                        spawnedPermit.issueMonth += 12;
                        spawnedPermit.issueYear -= 1;
                    }
                }
                else
                {
                    // Λάθος Σκοπός Ταξιδιού
                    spawnedPassport.GenerateData(false);
                    string[] badPurposes = { "Visit", "Work", "Transit" };
                    spawnedPassport.currentPurpose = badPurposes[Random.Range(0, badPurposes.Length)];

                    spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                    spawnedPermit.currentCity = spawnedPassport.originCityName;
                    spawnedPermit.issueDay = spawnedPassport.issueDay;
                    spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                    spawnedPermit.issueYear = spawnedPassport.issueYear;
                    if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                }
            }
            spawnedPassport.UpdateUI();
            spawnedPermit.UpdateUI();
        }
        else if (spawnedWrit != null)
        {
            // ΣΤΡΑΤΙΩΤΗΣ (Δεν αλλάζει κάτι)
        }
        else if (spawnedPassport != null)
        {
            // ΑΠΛΟΣ ΠΟΛΙΤΗΣ
            bool isCitizenForged = Random.value < citizenForgeryProbability;
            spawnedPassport.GenerateData(isCitizenForged);
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
                    if (IsDocumentStamped(itemInSocket)) stampedCount++;
                }
            }
            if (stampedCount >= totalRequired && totalRequired > 0) readyToLeave = true;
            yield return null;
        }

        yield return new WaitForSeconds(1.0f);
        EvaluatePlayerDecision();

        foreach (var doc in spawnedDocuments) if (doc != null) Destroy(doc);
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void EvaluatePlayerDecision()
    {
        DynamicPassport passport = null; MerchantPermit permit = null; MercenaryWrit writ = null;
        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();

        foreach (var socket in clientSockets)
        {
            if (socket != null && socket.hasSelection)
            {
                GameObject item = socket.GetOldestInteractableSelected().transform.gameObject;
                if (item.GetComponent<DynamicPassport>() != null) passport = item.GetComponent<DynamicPassport>();
                if (item.GetComponent<MerchantPermit>() != null) permit = item.GetComponent<MerchantPermit>();
                if (item.GetComponent<MercenaryWrit>() != null) writ = item.GetComponent<MercenaryWrit>();
            }
        }

        if (passport != null && permit != null)
        {
            bool namesMatch = (passport.currentFirstName == permit.currentFirstName) && (passport.currentLastName == permit.currentLastName);
            bool citiesMatch = (passport.originCityName == permit.currentCity);
            bool purposeMatch = (passport.currentPurpose == "Trade");

            bool permitNotTooEarly = CompareDates(passport.issueDay, passport.issueMonth, passport.issueYear, permit.issueDay, permit.issueMonth, permit.issueYear) <= 0;
            bool permitNotTooLate = CompareDates(permit.issueDay, permit.issueMonth, permit.issueYear, passport.expDay, passport.expMonth, passport.expYear) <= 0;
            bool datesMatch = permitNotTooEarly && permitNotTooLate;

            bool passportValid = !passport.isExpired && !passport.hasCityMismatch;

            bool shouldBeApproved = namesMatch && citiesMatch && purposeMatch && datesMatch && passportValid;
            bool playerApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved && permit.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == playerApproved)
            {
                Debug.Log("<color=green>ΣΩΣΤΟ!</color> Ορθή απόφαση για τον Έμπορο.");
                if (scoreManager != null) scoreManager.AddScore();
            }
            else
            {
                Debug.Log($"<color=red>ΛΑΘΟΣ!</color> Λάθος στον Έμπορο! Αιτία -> Ονόματα: {namesMatch}, Πόλεις: {citiesMatch}, Σκοπός (Trade): {purposeMatch}, Ημερομηνίες: {datesMatch}, Διαβατήριο Νόμιμο: {passportValid}");
                if (scoreManager != null) scoreManager.SubtractScore();
            }
        }
        else if (writ != null)
        {
            bool shouldBeApproved = !writ.isForged;
            bool playerApproved = (writ.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == playerApproved) { if (scoreManager != null) scoreManager.AddScore(); }
            else { if (scoreManager != null) scoreManager.SubtractScore(); }
        }
        else if (passport != null)
        {
            bool shouldBeApproved = !passport.isExpired && !passport.hasCityMismatch;
            bool playerApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == playerApproved) { if (scoreManager != null) scoreManager.AddScore(); }
            else { if (scoreManager != null) scoreManager.SubtractScore(); }
        }
    }

    private int CompareDates(int d1, int m1, int y1, int d2, int m2, int y2)
    {
        if (y1 != y2) return y1.CompareTo(y2);
        if (m1 != m2) return m1.CompareTo(m2);
        return d1.CompareTo(d2);
    }

    private bool IsDocumentStamped(GameObject item)
    {
        if (item.GetComponent<DynamicPassport>() != null) return item.GetComponent<DynamicPassport>().hasBeenStamped;
        if (item.GetComponent<MerchantPermit>() != null) return item.GetComponent<MerchantPermit>().hasBeenStamped;
        if (item.GetComponent<MercenaryWrit>() != null) return item.GetComponent<MercenaryWrit>().hasBeenStamped;
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