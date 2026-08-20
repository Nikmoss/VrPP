using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class DynamicPassport : MonoBehaviour
{
    [Header("Οπτικά Ανοιγοκλεισίματος")]
    public GameObject closedVisual;
    public GameObject openVisual;
    public AudioSource flipSound;

    [Header("UI References (Texts)")]
    public TMP_Text titleText;
    public TMP_Text paragraphText;
    public TMP_Text statsText;

    [Header("UI References (Images)")]
    public Image cityEmblemImage;

    [Header("Κατάσταση")]
    public string currentFirstName;
    public string currentLastName;
    public string originCityName;    // Η πραγματική πόλη (για το έμβλημα)
    public string writtenCityName;   // Η πόλη που αναγράφεται στο κείμενο
    public string currentPurpose;
    public string dest;
    public bool isForged;
    public bool isExpired;
    public bool hasCityMismatch;

    [Header("Ακριβείς Ημερομηνίες")]
    public int issueDay;
    public int issueMonth;
    public int issueYear;
    public int expDay;
    public int expMonth;
    public int expYear;

    [Header("Κατάσταση Σφραγίδας")]
    public bool hasBeenStamped = false;
    public VelocityStampTool.StampDecision lastAppliedStamp;

    public readonly string[] firstNames = { "NIKOLAS", "THOMAS", "WILLIAM", "JOHN", "EDWARD", "ROBERT", "MARY", "ELIZABETH", "ANNE" };
    public readonly string[] lastNames = { "MOSS", "SMITH", "BAKER", "CLARK", "WRIGHT", "TURNER", "COOPER" };
    public readonly string[] destinations = { "VOLOS", "ATHENS", "SPARTA", "THEBES", "CORINTH", };
    public readonly string[] purposes = { "Visit", "Trade", "Work", "Transit" };
    public readonly string[] cities = { "Lamia", "ATHENS", "THESSALONIKI", "VOLOS", "LARISSA" };

    [Header("Εμβλήματα Πόλεων")]
    public Sprite[] cityEmblems;

    private XRBaseInteractable interactable;
    private bool isOpen = false;

    private void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();
        GenerateData(false);
        UpdateVisuals();
    }

    private void OnEnable() { interactable.activated.AddListener(OnTriggerPressed); }
    private void OnDisable() { interactable.activated.RemoveListener(OnTriggerPressed); }
    private void OnTriggerPressed(ActivateEventArgs args) { TogglePassport(); }

    public void TogglePassport()
    {
        isOpen = !isOpen;
        if (flipSound != null) flipSound.Play();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (closedVisual != null) closedVisual.SetActive(!isOpen);
        if (openVisual != null) openVisual.SetActive(isOpen);
    }

    public void GenerateData(bool makeItForged)
    {
        currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
        currentLastName = lastNames[Random.Range(0, lastNames.Length)];
        dest = destinations[Random.Range(0, destinations.Length)];
        currentPurpose = purposes[Random.Range(0, purposes.Length)];

        isForged = makeItForged;
        isExpired = false;
        hasCityMismatch = false;

        int gameYear = 2026;
        float expiredChance = 0.75f;

        if (DayManager.Instance != null)
        {
            DaySettings today = DayManager.Instance.GetCurrentDaySettings();
            if (today != null)
            {
                gameYear = today.currentGameYear;
                expiredChance = today.expiredErrorChance;
            }
        }

        if (isForged)
        {
            if (Random.value < expiredChance) isExpired = true;
            else hasCityMismatch = true;
        }

        expYear = isExpired ? Random.Range(gameYear - 3, gameYear) : Random.Range(gameYear + 1, gameYear + 5);
        expMonth = Random.Range(1, 13);
        expDay = Random.Range(1, 29);

        issueYear = expYear - Random.Range(3, 6);
        issueMonth = Random.Range(1, 13);
        issueDay = Random.Range(1, 29);

        int cityIndex = Random.Range(0, cities.Length);
        originCityName = cities[cityIndex];
        writtenCityName = originCityName; // Αρχικά είναι ίδια

        // Αν είναι πλαστό, αλλάζουμε ΤΟ ΚΕΙΜΕΝΟ, όχι το έμβλημα
        if (hasCityMismatch)
        {
            int wrongIndex = Random.Range(0, cities.Length);
            while (wrongIndex == cityIndex) wrongIndex = Random.Range(0, cities.Length);
            writtenCityName = cities[wrongIndex];
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        string expiryDateStr = $"{GetOrdinal(expDay)} day of {GetMonthName(expMonth)}, {expYear}";
        string issueDateStr = $"{GetOrdinal(issueDay)} day of {GetMonthName(issueMonth)}, {issueYear}";

        // Το Έμβλημα διαβάζει την πραγματική πόλη (originCityName)
        if (cityEmblemImage != null)
        {
            int cityIndex = System.Array.IndexOf(cities, originCityName);
            if (cityIndex >= 0 && cityEmblems != null && cityIndex < cityEmblems.Length)
            {
                cityEmblemImage.sprite = cityEmblems[cityIndex];
                cityEmblemImage.color = Color.white;
            }
        }

        if (titleText != null) titleText.text = "Letter of Safe Conduct";
        if (paragraphText != null) paragraphText.text = "Γνωστοποιείται ότι ο κομιστής\nτου παρόντος, που ονομάζεται\nπαρακάτω, είναι ειρηνικός\nταξιδιώτης. Επιτρέπεται η\nελεύθερη διέλευσή του χωρίς\nκώλυμα.";

        // Το Κείμενο τυπώνει τη γραπτή πόλη (writtenCityName)
        if (statsText != null)
        {
            statsText.text =
                $"Name of the Bearer: {currentFirstName} {currentLastName}\n" +
                $"Place of Origin: {writtenCityName}\n" +
                $"Destination: {dest}\n" +
                $"Purpose of Travel: {currentPurpose}\n" +
                $"Accompanied by: None\n" +
                $"Date Issued: {issueDateStr}\n" +
                $"Valid Until: {expiryDateStr}";
        }
    }

    public void SetStampDecision(VelocityStampTool.StampDecision decision)
    {
        hasBeenStamped = true;
        lastAppliedStamp = decision;
    }
    private string GetOrdinal(int num)
    {
        if (num <= 0) return num.ToString();
        switch (num % 100) { case 11: case 12: case 13: return num + "th"; }
        switch (num % 10) { case 1: return num + "st"; case 2: return num + "nd"; case 3: return num + "rd"; default: return num + "th"; }
    }

    private string GetMonthName(int month)
    {
        string[] months = { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        if (month >= 1 && month <= 12) return months[month];
        return "Unknown";
    }
}