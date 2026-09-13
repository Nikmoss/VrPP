using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private EncounterSetup currentEncounter;
    private GameObject spawnedBribeItem;

    public void Setup(Transform spawn, Transform window, Transform exit, GameObject[] docs, XRSocketInteractor[] sockets, EncounterSetup encounter = null)
    {
        spawnPoint = spawn;
        windowPoint = window;
        exitPoint = exit;
        documentPrefabs = docs;
        clientSockets = sockets;

        currentEncounter = encounter;

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

        DaySettings today = DayManager.Instance != null ? DayManager.Instance.GetCurrentDaySettings() : null;
        float citProb = today != null ? today.citizenForgeryProbability : 0f;
        float merProb = today != null ? today.merchantForgeryProbability : 0f;

        bool isScripted = currentEncounter != null && (currentEncounter.npcType == NPCType.ScriptedCitizen || currentEncounter.npcType == NPCType.ScriptedMerchant);
        CityStatsManager.ErrorType forcedError = isScripted ? currentEncounter.forcedError : CityStatsManager.ErrorType.None;

        if (isScripted && currentEncounter.bribeItemPrefab != null)
        {
            Vector3 bribePos = windowPoint.position + new Vector3(0.4f, 0, 0);
            spawnedBribeItem = Instantiate(currentEncounter.bribeItemPrefab, bribePos, Quaternion.identity);
        }

        // ==========================================
        // ΓΕΝΝΗΤΡΙΑ ΕΓΓΡΑΦΩΝ 
        // ==========================================
        if (spawnedPassport != null && spawnedPermit != null) // ΕΜΠΟΡΟΣ
        {
            if (isScripted)
            {
                int failSafe = 100;
                do
                {
                    spawnedPassport.GenerateData(false);
                    failSafe--;
                } while (today != null && !string.IsNullOrEmpty(today.bannedCity) && spawnedPassport.originCityName == today.bannedCity && failSafe > 0);

                spawnedPassport.currentPurpose = "Trade";
                spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                spawnedPermit.currentCity = spawnedPassport.originCityName;
                spawnedPermit.writtenCityName = spawnedPassport.originCityName;
                spawnedPermit.hasCityMismatch = false;
                spawnedPermit.issueDay = spawnedPassport.issueDay;
                spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                spawnedPermit.issueYear = spawnedPassport.issueYear;
                if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }

                if (forcedError == CityStatsManager.ErrorType.Expired)
                {
                    spawnedPassport.isExpired = true;
                    int currentYear = today != null ? today.currentGameYear : 2026;
                    spawnedPassport.expYear = currentYear - Random.Range(1, 3);
                    spawnedPassport.issueYear = spawnedPassport.expYear - 5;
                }
                else if (forcedError == CityStatsManager.ErrorType.WrongName)
                {
                    int wrongFirstIndex = (System.Array.IndexOf(spawnedPermit.firstNames, spawnedPassport.currentFirstName) + 1) % spawnedPermit.firstNames.Length;
                    spawnedPermit.currentFirstName = spawnedPermit.firstNames[wrongFirstIndex];
                }
                else if (forcedError == CityStatsManager.ErrorType.WrongEmblem)
                {
                    int correctIndex = System.Array.IndexOf(spawnedPermit.cities, spawnedPassport.originCityName);
                    int wrongEmblemIndex = Random.Range(0, spawnedPermit.cities.Length);
                    while (wrongEmblemIndex == correctIndex) wrongEmblemIndex = Random.Range(0, spawnedPermit.cities.Length);
                    spawnedPermit.currentCity = spawnedPermit.cities[wrongEmblemIndex];
                }
                else if (forcedError == CityStatsManager.ErrorType.DifCity)
                {
                    int wrongCityIndex = (System.Array.IndexOf(spawnedPermit.cities, spawnedPassport.originCityName) + 1) % spawnedPermit.cities.Length;
                    spawnedPermit.writtenCityName = spawnedPermit.cities[wrongCityIndex];
                }
                else if (forcedError == CityStatsManager.ErrorType.BannedRule)
                {
                    if (today != null && !string.IsNullOrEmpty(today.bannedCity))
                    {
                        spawnedPassport.originCityName = today.bannedCity;
                        spawnedPassport.writtenCityName = today.bannedCity;
                        spawnedPermit.currentCity = today.bannedCity;
                        spawnedPermit.writtenCityName = today.bannedCity;
                    }
                }
            }
            else // ΤΥΧΑΙΟΣ ΕΜΠΟΡΟΣ
            {
                bool isMerchantForged = Random.value < merProb;
                if (!isMerchantForged)
                {
                    spawnedPassport.GenerateData(false);
                    spawnedPassport.currentPurpose = "Trade";
                    spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                    spawnedPermit.currentCity = spawnedPassport.originCityName;
                    spawnedPermit.writtenCityName = spawnedPassport.originCityName;
                    spawnedPermit.hasCityMismatch = false;
                    spawnedPermit.issueDay = spawnedPassport.issueDay;
                    spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                    spawnedPermit.issueYear = spawnedPassport.issueYear;
                    if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                }
                else
                {
                    float wPassport = today != null ? today.weightPassportError : 20f;
                    float wName = today != null ? today.weightNameError : 20f;
                    float wCity = today != null ? today.weightCityError : 20f;
                    float wDate = today != null ? today.weightDateError : 20f;
                    float wPurpose = today != null ? today.weightPurposeError : 20f;
                    float wEmblem = today != null ? today.weightPermitEmblemError : 20f;
                    float totalWeight = wPassport + wName + wCity + wDate + wPurpose + wEmblem;
                    float randomWeight = Random.Range(0f, totalWeight);

                    spawnedPermit.hasCityMismatch = false;

                    if (randomWeight < wPassport)
                    {
                        spawnedPassport.GenerateData(true);
                        spawnedPassport.currentPurpose = "Trade";
                        spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                        spawnedPermit.currentCity = spawnedPassport.originCityName;
                        spawnedPermit.writtenCityName = spawnedPassport.originCityName;
                        spawnedPermit.issueDay = spawnedPassport.issueDay;
                        spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                        spawnedPermit.issueYear = spawnedPassport.issueYear;
                        if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                    }
                    else if (randomWeight < wPassport + wName)
                    {
                        spawnedPassport.GenerateData(false);
                        spawnedPassport.currentPurpose = "Trade";
                        int wrongFirstIndex = (System.Array.IndexOf(spawnedPermit.firstNames, spawnedPassport.currentFirstName) + 1) % spawnedPermit.firstNames.Length;
                        spawnedPermit.currentFirstName = spawnedPermit.firstNames[wrongFirstIndex];
                        spawnedPermit.currentLastName = spawnedPassport.currentLastName;
                        spawnedPermit.currentCity = spawnedPassport.originCityName;
                        spawnedPermit.writtenCityName = spawnedPassport.originCityName;
                        spawnedPermit.issueDay = spawnedPassport.issueDay;
                        spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                        spawnedPermit.issueYear = spawnedPassport.issueYear;
                        if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                    }
                    else if (randomWeight < wPassport + wName + wCity)
                    {
                        spawnedPassport.GenerateData(false);
                        spawnedPassport.currentPurpose = "Trade";
                        spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                        spawnedPermit.currentCity = spawnedPassport.originCityName;
                        int wrongCityIndex = (System.Array.IndexOf(spawnedPermit.cities, spawnedPassport.originCityName) + 1) % spawnedPermit.cities.Length;
                        spawnedPermit.writtenCityName = spawnedPermit.cities[wrongCityIndex];
                        spawnedPermit.issueDay = spawnedPassport.issueDay;
                        spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                        spawnedPermit.issueYear = spawnedPassport.issueYear;
                        if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                    }
                    else if (randomWeight < wPassport + wName + wCity + wDate)
                    {
                        spawnedPassport.GenerateData(false);
                        spawnedPassport.currentPurpose = "Trade";
                        spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                        spawnedPermit.currentCity = spawnedPassport.originCityName;
                        spawnedPermit.writtenCityName = spawnedPassport.originCityName;
                        spawnedPermit.issueDay = spawnedPassport.issueDay;
                        spawnedPermit.issueYear = spawnedPassport.issueYear;
                        spawnedPermit.issueMonth = spawnedPassport.issueMonth - Random.Range(1, 4);
                        if (spawnedPermit.issueMonth <= 0) { spawnedPermit.issueMonth += 12; spawnedPermit.issueYear -= 1; }
                    }
                    else if (randomWeight < wPassport + wName + wCity + wDate + wPurpose)
                    {
                        spawnedPassport.GenerateData(false);
                        string[] badPurposes = { "Visit", "Work", "Transit" };
                        spawnedPassport.currentPurpose = badPurposes[Random.Range(0, badPurposes.Length)];
                        spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                        spawnedPermit.currentCity = spawnedPassport.originCityName;
                        spawnedPermit.writtenCityName = spawnedPassport.originCityName;
                        spawnedPermit.issueDay = spawnedPassport.issueDay;
                        spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                        spawnedPermit.issueYear = spawnedPassport.issueYear;
                        if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                    }
                    else
                    {
                        spawnedPassport.GenerateData(false);
                        spawnedPassport.currentPurpose = "Trade";
                        spawnedPermit.ForceNames(spawnedPassport.currentFirstName, spawnedPassport.currentLastName);
                        spawnedPermit.writtenCityName = spawnedPassport.originCityName;
                        int correctIndex = System.Array.IndexOf(spawnedPermit.cities, spawnedPassport.originCityName);
                        int wrongEmblemIndex = Random.Range(0, spawnedPermit.cities.Length);
                        while (wrongEmblemIndex == correctIndex) wrongEmblemIndex = Random.Range(0, spawnedPermit.cities.Length);
                        spawnedPermit.currentCity = spawnedPermit.cities[wrongEmblemIndex];
                        spawnedPermit.issueDay = spawnedPassport.issueDay;
                        spawnedPermit.issueMonth = spawnedPassport.issueMonth + 1;
                        spawnedPermit.issueYear = spawnedPassport.issueYear;
                        if (spawnedPermit.issueMonth > 12) { spawnedPermit.issueMonth -= 12; spawnedPermit.issueYear += 1; }
                    }
                }
            }
            spawnedPassport.UpdateUI();
            spawnedPermit.UpdateUI();
        }
        else if (spawnedWrit != null)
        {
            // ΣΤΡΑΤΙΩΤΗΣ
        }
        else if (spawnedPassport != null) // ΑΠΛΟΣ ΠΟΛΙΤΗΣ
        {
            if (isScripted)
            {
                if (forcedError == CityStatsManager.ErrorType.Expired)
                {
                    // Πάντα ξεκινάμε με ένα 100% καθαρό χαρτί (που ΔΕΝ είναι η απαγορευμένη πόλη)
                    int failSafe = 100;
                    do
                    {
                        spawnedPassport.GenerateData(false);
                        failSafe--;
                    } while (today != null && !string.IsNullOrEmpty(today.bannedCity) && spawnedPassport.originCityName == today.bannedCity && failSafe > 0);

                    // Μετά το λήγουμε
                    spawnedPassport.isExpired = true;
                    int currentYear = today != null ? today.currentGameYear : 2026;
                    spawnedPassport.expYear = currentYear - Random.Range(1, 3);
                    spawnedPassport.issueYear = spawnedPassport.expYear - 5;
                }
                else if (forcedError == CityStatsManager.ErrorType.WrongEmblem)
                {
                    // Βάζουμε το διαβατήριο να ρολάρει ΜΕΧΡΙ να πετύχει λάθος έμβλημα (και να ΜΗΝ είναι ληγμένο)
                    int failSafe = 100;
                    do
                    {
                        spawnedPassport.GenerateData(true);
                        failSafe--;
                    } while ((spawnedPassport.isExpired || !spawnedPassport.hasCityMismatch) && failSafe > 0);
                }
                else if (forcedError == CityStatsManager.ErrorType.BannedRule)
                {
                    spawnedPassport.GenerateData(false);
                    if (today != null && !string.IsNullOrEmpty(today.bannedCity))
                    {
                        spawnedPassport.originCityName = today.bannedCity;
                        spawnedPassport.writtenCityName = today.bannedCity;
                    }
                }
                else
                {
                    // None (Καθαρό χαρτί)
                    int failSafe = 100;
                    do
                    {
                        spawnedPassport.GenerateData(false);
                        failSafe--;
                    } while (today != null && !string.IsNullOrEmpty(today.bannedCity) && spawnedPassport.originCityName == today.bannedCity && failSafe > 0);
                }
            }
            else // Τυχαίος Πολίτης
            {
                bool isCitizenForged = Random.value < citProb;
                if (isCitizenForged)
                {
                    float expChance = today != null ? today.expiredErrorChance : 0.75f;
                    if (Random.value < expChance)
                    {
                        spawnedPassport.GenerateData(false);
                        spawnedPassport.isExpired = true;
                        int currentYear = today != null ? today.currentGameYear : 2026;
                        spawnedPassport.expYear = currentYear - Random.Range(1, 3);
                        spawnedPassport.issueYear = spawnedPassport.expYear - 5;
                    }
                    else
                    {
                        int failSafe = 100;
                        do
                        {
                            spawnedPassport.GenerateData(true);
                            failSafe--;
                        } while ((spawnedPassport.isExpired || !spawnedPassport.hasCityMismatch) && failSafe > 0);
                    }
                }
                else
                {
                    spawnedPassport.GenerateData(false);
                }
            }

            spawnedPassport.UpdateUI();
        }

        // ==========================================
        // ΑΝΑΜΟΝΗ ΓΙΑ ΣΦΡΑΓΙΣΜΑ
        // ==========================================
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

        EvaluatePlayerDecision(today);

        // --- ΕΛΕΓΧΟΣ ΑΠΟΔΟΧΗΣ ΤΗΣ ΔΩΡΟΔΟΚΙΑΣ ---
        bool playerApprovedHim = false;
        if (spawnedPassport != null && spawnedPermit != null)
        {
            playerApprovedHim = (spawnedPassport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved && spawnedPermit.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);
        }
        else if (spawnedPassport != null)
        {
            playerApprovedHim = (spawnedPassport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);
        }

        if (!playerApprovedHim && spawnedBribeItem != null)
        {
            Destroy(spawnedBribeItem); // Αν τον έδιωξες, παίρνει το δώρο πίσω
        }

        foreach (var doc in spawnedDocuments) if (doc != null) Destroy(doc);
        while (Vector3.Distance(transform.position, exitPoint.position) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, exitPoint.position, moveSpeed * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    private void EvaluatePlayerDecision(DaySettings todaySettings)
    {
        DynamicPassport passport = null;
        MerchantPermit permit = null;
        MercenaryWrit writ = null;

        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        CityStatsManager cityStats = FindObjectOfType<CityStatsManager>();

        string bannedCity = todaySettings != null ? todaySettings.bannedCity : "";
        string bannedItem = todaySettings != null ? todaySettings.bannedItem : "";

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
            bool purposeMatch = (passport.currentPurpose == "Trade");

            bool permitNotTooEarly = CompareDates(passport.issueDay, passport.issueMonth, passport.issueYear, permit.issueDay, permit.issueMonth, permit.issueYear) <= 0;
            bool permitNotTooLate = CompareDates(permit.issueDay, permit.issueMonth, permit.issueYear, passport.expDay, passport.expMonth, passport.expYear) <= 0;
            bool datesMatch = permitNotTooEarly && permitNotTooLate;

            bool passportValid = !passport.isExpired && !passport.hasCityMismatch;
            bool permitValid = !permit.hasCityMismatch;

            bool citiesMatch = (passport.originCityName == permit.currentCity) && (passport.writtenCityName == permit.writtenCityName);

            bool isCityBanned = (bannedCity != "" && (passport.originCityName == bannedCity || permit.currentCity == bannedCity));
            bool carriesBannedItem = (bannedItem != "" && permit.goodsText != null && permit.goodsText.text.Contains(bannedItem));
            bool breaksRuleboard = isCityBanned || carriesBannedItem;

            bool shouldBeApproved = namesMatch && citiesMatch && purposeMatch && datesMatch && passportValid && permitValid && !breaksRuleboard;
            bool playerApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved && permit.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == playerApproved)
            {
                if (scoreManager != null) scoreManager.AddScore();
            }
            else if (!shouldBeApproved && playerApproved)
            {
                if (scoreManager != null) scoreManager.SubtractScore();
                if (cityStats != null)
                {
                    if (!namesMatch) cityStats.ReportError(CityStatsManager.ErrorType.WrongName);
                    else if (!datesMatch || passport.isExpired) cityStats.ReportError(CityStatsManager.ErrorType.Expired);
                    else if (passport.originCityName != permit.currentCity || !passportValid || !permitValid) cityStats.ReportError(CityStatsManager.ErrorType.WrongEmblem);
                    else if (passport.writtenCityName != permit.writtenCityName) cityStats.ReportError(CityStatsManager.ErrorType.DifCity);
                    else if (breaksRuleboard) cityStats.ReportError(CityStatsManager.ErrorType.BannedRule);
                }
            }
            else if (shouldBeApproved && !playerApproved)
            {
                if (scoreManager != null) scoreManager.SubtractScore();
                if (cityStats != null) cityStats.ModifyStats(0, -5, 0, 0);
            }
        }
        else if (passport != null)
        {
            bool isCityBanned = (bannedCity != "" && passport.originCityName == bannedCity);
            bool shouldBeApproved = !passport.isExpired && !passport.hasCityMismatch && !isCityBanned;
            bool playerApproved = (passport.lastAppliedStamp == VelocityStampTool.StampDecision.Approved);

            if (shouldBeApproved == playerApproved)
            {
                if (scoreManager != null) scoreManager.AddScore();
            }
            else if (!shouldBeApproved && playerApproved)
            {
                if (scoreManager != null) scoreManager.SubtractScore();
                if (cityStats != null)
                {
                    if (passport.isExpired) cityStats.ReportError(CityStatsManager.ErrorType.Expired);
                    else if (passport.hasCityMismatch) cityStats.ReportError(CityStatsManager.ErrorType.WrongEmblem);
                    else if (isCityBanned) cityStats.ReportError(CityStatsManager.ErrorType.BannedRule);
                }
            }
            else if (shouldBeApproved && !playerApproved)
            {
                if (scoreManager != null) scoreManager.SubtractScore();
                if (cityStats != null) cityStats.ModifyStats(0, -5, 0, 0);
            }
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

    public void ResetDocumentsToSockets()
    {
        for (int i = 0; i < spawnedDocuments.Count; i++)
        {
            if (spawnedDocuments[i] != null && clientSockets != null && i < clientSockets.Length && clientSockets[i] != null)
            {
                Rigidbody rb = spawnedDocuments[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                spawnedDocuments[i].transform.position = clientSockets[i].transform.position;
                spawnedDocuments[i].transform.rotation = clientSockets[i].transform.rotation;
            }
        }
    }
}