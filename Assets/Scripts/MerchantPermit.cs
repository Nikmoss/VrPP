using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MerchantPermit : MonoBehaviour
{
    [Header("UI References (Texts)")]
    public TMP_Text titleText;
    public TMP_Text nameText;
    public TMP_Text paragraphText;
    public TMP_Text dateText;
    public TMP_Text goodsText;

    [Header("UI References (Images)")]
    public Image cityEmblemImage;

    [Header("Κατάσταση")]
    public string currentFirstName;
    public string currentLastName;
    public string currentCity;       // Η πραγματική πόλη (για το έμβλημα)
    public string writtenCityName;   // Η πόλη που αναγράφεται στο κείμενο
    public bool hasCityMismatch = false;

    [Header("Ακριβείς Ημερομηνίες")]
    public int issueDay;
    public int issueMonth;
    public int issueYear;

    [Header("Κατάσταση Σφραγίδας")]
    public bool hasBeenStamped = false;
    public VelocityStampTool.StampDecision lastAppliedStamp;

    public readonly string[] firstNames = { "NIKOLAS", "THOMAS", "WILLIAM", "JOHN", "EDWARD", "ROBERT", "MARY", "ELIZABETH", "ANNE" };
    public readonly string[] lastNames = { "MOSS", "SMITH", "BAKER", "CLARK", "WRIGHT", "TURNER", "COOPER" };
    public readonly string[] cities = { "Lamia", "ATHENS", "THESSALONIKI", "VOLOS", "LARISSA" };
    private readonly string[] items = { "Fine Wool (Rolls)", "Salt (Barrels)", "Dried Fruits (Chests)", "Spices (Sacks)", "Olive Oil (Jars)", "Copper Ingots", "Silver Coins" };

    [Header("Εμβλήματα Πόλεων")]
    public Sprite[] cityEmblems;

    private void Awake()
    {
        GenerateData();
    }

    public void GenerateData()
    {
        currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
        currentLastName = lastNames[Random.Range(0, lastNames.Length)];

        int cityIndex = Random.Range(0, cities.Length);
        currentCity = cities[cityIndex];
        writtenCityName = currentCity; // Αρχικά είναι ίδια

        hasCityMismatch = false;

        issueDay = Random.Range(1, 29);
        issueMonth = Random.Range(1, 13);

        int gameYear = 2026;
        if (DayManager.Instance != null)
        {
            DaySettings today = DayManager.Instance.GetCurrentDaySettings();
            if (today != null) gameYear = today.currentGameYear;
        }

        issueYear = gameYear - Random.Range(1, 4);

        UpdateUI();
    }

    // Μέθοδος για να το καλεί ο NPCController αν θέλει να δημιουργήσει πλαστογραφία
    public void ApplyCityMismatch()
    {
        hasCityMismatch = true;
        int cityIndex = System.Array.IndexOf(cities, currentCity);
        int wrongIndex = Random.Range(0, cities.Length);

        while (wrongIndex == cityIndex)
        {
            wrongIndex = Random.Range(0, cities.Length);
        }

        writtenCityName = cities[wrongIndex];
        UpdateUI();
    }

    public void ForceNames(string fName, string lName)
    {
        currentFirstName = fName;
        currentLastName = lName;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (titleText != null) titleText.text = "Merchant's Manifest";
        if (nameText != null) nameText.text = $"{currentFirstName} {currentLastName}";

        // Το Κείμενο τυπώνει τη γραπτή πόλη (writtenCityName)
        if (paragraphText != null)
        {
            paragraphText.text = $"Be it Known that the bearer\n\n\n\na registered merchant\nof the City of {writtenCityName}, is authorised to travel\nand trade within the Kingdom's lands";
        }

        if (dateText != null)
        {
            dateText.text = $"Date Issued\n{GetOrdinal(issueDay)} day of {GetMonthName(issueMonth)}, {issueYear}";
        }

        if (goodsText != null)
        {
            int numberOfItems = Random.Range(2, 5);
            string inventoryString = "Goods Carried\n\n";
            for (int i = 0; i < numberOfItems; i++)
            {
                inventoryString += $"{items[Random.Range(0, items.Length)]} ........... {Random.Range(5, 50)}\n";
            }
            goodsText.text = inventoryString;
        }

        // Το Έμβλημα διαβάζει την πραγματική πόλη (currentCity)
        if (cityEmblemImage != null)
        {
            int cityIndex = System.Array.IndexOf(cities, currentCity);
            if (cityIndex >= 0 && cityEmblems != null && cityIndex < cityEmblems.Length)
            {
                cityEmblemImage.sprite = cityEmblems[cityIndex];
                cityEmblemImage.color = Color.white;
            }
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