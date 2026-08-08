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
    public TMP_Text signatureText;

    [Header("UI References (Images)")]
    public Image cityEmblemImage;
    public Image signatureImage;

    [Header("Κατάσταση")]
    public string currentFirstName;
    public string currentLastName;
    public string currentCity;

    [Header("Ακριβείς Ημερομηνίες")]
    public int issueDay;
    public int issueMonth;
    public int issueYear;

    [Header("Κατάσταση Σφραγίδας")]
    public bool hasBeenStamped = false;
    public VelocityStampTool.StampDecision lastAppliedStamp;

    public readonly string[] firstNames = { "NIKOLAS", "THOMAS", "WILLIAM", "JOHN", "EDWARD", "ROBERT", "MARY", "ELIZABETH", "ANNE" };
    public readonly string[] lastNames = { "MOSS", "SMITH", "BAKER", "CLARK", "WRIGHT", "TURNER", "COOPER" };
    public readonly string[] cities = { "VOLOS", "ATHENS", "SPARTA", "THEBES", "CORINTH" };

    private readonly Color[] cityColors = { Color.blue, new Color(0f, 0.5f, 0f), Color.red, Color.magenta, Color.gray };
    private readonly Color[] signatureColors = { Color.black, new Color(0.1f, 0.1f, 0.4f), new Color(0.4f, 0.1f, 0.1f), new Color(0.2f, 0.2f, 0.2f) };
    private readonly string[] items = { "Fine Wool (Rolls)", "Salt (Barrels)", "Dried Fruits (Chests)", "Spices (Sacks)", "Olive Oil (Jars)", "Copper Ingots", "Silver Coins" };

    private void Awake()
    {
        GenerateData();
    }

    public void GenerateData()
    {
        currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
        currentLastName = lastNames[Random.Range(0, lastNames.Length)];
        currentCity = cities[Random.Range(0, cities.Length)];

        issueDay = Random.Range(1, 29);
        issueMonth = Random.Range(1, 13);
        issueYear = 2026 - Random.Range(1, 4);

        UpdateUI();
    }

    public void ForceNames(string fName, string lName)
    {
        currentFirstName = fName;
        currentLastName = lName;
        UpdateUI();
    }

    // ΝΕΟ: Εγγυάται ότι θα διαλέξει ΟΠΩΣΔΗΠΟΤΕ λάθος ονόματα
    public void ForceDifferentNames(string correctFirst, string correctLast)
    {
        do
        {
            currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
            currentLastName = lastNames[Random.Range(0, lastNames.Length)];
        }
        while (currentFirstName == correctFirst && currentLastName == correctLast);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (titleText != null) titleText.text = "Merchant's Manifest";
        if (nameText != null) nameText.text = $"{currentFirstName} {currentLastName}";

        if (paragraphText != null)
        {
            paragraphText.text = $"Be it Known that the bearer\n\n\n\na registered merchant\nof the City of {currentCity}, is authorised to travel\nand trade within the Kingdom's lands";
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

        if (signatureText != null) signatureText.text = "By the Authority of the Guild,\n\nWarden of the Guild";

        if (cityEmblemImage != null)
        {
            int cityIndex = System.Array.IndexOf(cities, currentCity);
            if (cityIndex >= 0 && cityIndex < cityColors.Length) cityEmblemImage.color = cityColors[cityIndex];
        }

        if (signatureImage != null) signatureImage.color = signatureColors[Random.Range(0, signatureColors.Length)];
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