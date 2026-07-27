using UnityEngine;
using TMPro;

/// <summary>
/// Δημιουργεί τυχαία στοιχεία άδειας εμπόρου (ονόματα, εμπόρευμα), 
/// ενημερώνει το Canvas και "θυμάται" την τελευταία σφραγίδα που δέχτηκε.
/// </summary>
public class MerchantPermit : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Το TextMeshPro που δείχνει τα στοιχεία της άδειας.")]
    public TMP_Text permitText;

    [Header("Κατάσταση (Διαβάζεται από άλλα scripts)")]
    public string currentFirstName;
    public string currentLastName;
    public string currentGoods;

    [Header("Κατάσταση Σφραγίδας")]
    public bool hasBeenStamped = false;
    public VelocityStampTool.StampDecision lastAppliedStamp;

    private readonly string[] firstNames = { "GREGOR", "IVAN", "ANNA", "MARIA", "DMITRI", "ELENA", "BORIS", "NATALIA", "YURI", "KATYA" };
    private readonly string[] lastNames = { "IVANOV", "SMIRNOV", "POPOV", "SOKOLOV", "VOLKOV", "KOZLOV", "MOROZOV", "NOVIKOV", "PETROV" };
    private readonly string[] goodsList = { "ΞΥΛΕΙΑ", "ΣΙΔΗΡΟΣ", "ΣΙΤΑΡΙ", "ΚΡΑΣΙ", "ΜΠΑΧΑΡΙΚΑ", "ΓΟΥΝΕΣ", "ΟΠΛΑ", "ΥΦΑΣΜΑΤΑ" };

    // ΑΛΛΑΓΗ: Το Awake εκτελείται ακαριαία πριν από τον έλεγχο του NPC
    private void Awake()
    {
        GenerateData();
    }

    public void GenerateData()
    {
        currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
        currentLastName = lastNames[Random.Range(0, lastNames.Length)];
        currentGoods = goodsList[Random.Range(0, goodsList.Length)];

        UpdateUI();
    }

    public void SetStampDecision(VelocityStampTool.StampDecision decision)
    {
        hasBeenStamped = true;
        lastAppliedStamp = decision;
    }

    public void ForceNames(string fName, string lName)
    {
        currentFirstName = fName;
        currentLastName = lName;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (permitText != null)
        {
            permitText.text = $"ΟΝΟΜΑ: {currentFirstName}\nΕΠΙΘΕΤΟ: {currentLastName}\nΕΜΠΟΡΕΥΜΑ: {currentGoods}";
        }
    }
}