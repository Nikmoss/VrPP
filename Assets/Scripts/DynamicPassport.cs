using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DynamicPassport : MonoBehaviour
{
    [Header("UI References (Texts)")]
    public TMP_Text titleText;
    public TMP_Text paragraphText;
    public TMP_Text statsText;
    public TMP_Text signatureText;

    [Header("UI References (Images)")]
    public Image cityEmblemImage;
    public Image lordSignatureImage;

    [Header("Κατάσταση (Διαβάζεται από άλλα scripts)")]
    public string currentFirstName;
    public string currentLastName;
    public string originCityName;
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
    public readonly string[] destinations = { "SALOUGA", "ATHENS", "THESSALONIKI", "VOLOS", "LARISSA" };
    public readonly string[] purposes = { "Visit", "Trade", "Work", "Transit" };

    public readonly string[] cities = { "VOLOS", "ATHENS", "SPARTA", "THEBES", "CORINTH" };
    private readonly Color[] cityColors = { Color.blue, new Color(0f, 0.5f, 0f), Color.red, Color.magenta, Color.gray };
    private readonly Color[] signatureColors = { Color.black, new Color(0.1f, 0.1f, 0.4f), new Color(0.4f, 0.1f, 0.1f), new Color(0.2f, 0.2f, 0.2f) };

    private void Awake()
    {
        GenerateData(false);
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

        // --- ΑΝΤΛΗΣΗ ΔΕΔΟΜΕΝΩΝ ΑΠΟ DAY MANAGER ---
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

        UpdateUI();
    }

    public void UpdateUI()
    {
        string expiryDateStr = $"{GetOrdinal(expDay)} day of {GetMonthName(expMonth)}, {expYear}";
        string issueDateStr = $"{GetOrdinal(issueDay)} day of {GetMonthName(issueMonth)}, {issueYear}";

        Color assignedCityColor = Color.white;
        int cityIndex = System.Array.IndexOf(cities, originCityName);

        if (cityIndex >= 0)
        {
            assignedCityColor = cityColors[cityIndex];

            if (hasCityMismatch)
            {
                int wrongColorIndex = Random.Range(0, cityColors.Length);
                while (wrongColorIndex == cityIndex) wrongColorIndex = Random.Range(0, cityColors.Length);
                assignedCityColor = cityColors[wrongColorIndex];
            }
        }

        if (cityEmblemImage != null) cityEmblemImage.color = assignedCityColor;

        if (titleText != null) titleText.text = "Letter of Safe Conduct";
        if (paragraphText != null) paragraphText.text = "Γνωστοποιείται ότι ο κομιστής\nτου παρόντος, που ονομάζεται\nπαρακάτω, είναι ειρηνικός\nταξιδιώτης. Επιτρέπεται η\nελεύθερη διέλευσή του χωρίς\nκώλυμα.";

        if (statsText != null)
        {
            statsText.text =
                $"Name of the Bearer: {currentFirstName} {currentLastName}\n" +
                $"Place of Origin: {originCityName}\n" +
                $"Destination: {dest}\n" +
                $"Purpose of Travel: {currentPurpose}\n" +
                $"Accompanied by: None\n" +
                $"Date Issued: {issueDateStr}\n" +
                $"Valid Until: {expiryDateStr}";
        }

        if (signatureText != null) signatureText.text = $"By order of the Sheriff of {originCityName},";
        if (lordSignatureImage != null) lordSignatureImage.color = signatureColors[Random.Range(0, signatureColors.Length)];
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