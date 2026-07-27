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

    // Ίδιες λίστες με το Passport για να υπάρχουν πιθανότητες να ταιριάζουν
    private readonly string[] firstNames = { "GREGOR", "IVAN", "ANNA", "MARIA", "DMITRI", "ELENA", "BORIS", "NATALIA", "YURI", "KATYA" };
    private readonly string[] lastNames = { "IVANOV", "SMIRNOV", "POPOV", "SOKOLOV", "VOLKOV", "KOZLOV", "MOROZOV", "NOVIKOV", "PETROV" };

    // Η νέα λίστα με τα εμπορεύματα
    private readonly string[] goodsList = { "ΞΥΛΕΙΑ", "ΣΙΔΗΡΟΣ", "ΣΙΤΑΡΙ", "ΚΡΑΣΙ", "ΜΠΑΧΑΡΙΚΑ", "ΓΟΥΝΕΣ", "ΟΠΛΑ", "ΥΦΑΣΜΑΤΑ" };

    private void Start()
    {
        GenerateData();
    }

    private void GenerateData()
    {
        // Επιλογή τυχαίων στοιχείων
        currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
        currentLastName = lastNames[Random.Range(0, lastNames.Length)];
        currentGoods = goodsList[Random.Range(0, goodsList.Length)];

        // Ενημέρωση του UI Text όπως ακριβώς και στο Passport
        if (permitText != null)
        {
            permitText.text = $"ΟΝΟΜΑ: {currentFirstName}\nΕΠΙΘΕΤΟ: {currentLastName}\nΕΜΠΟΡΕΥΜΑ: {currentGoods}";
        }
    }

    /// <summary>
    /// Καλείται από τη σφραγίδα κάθε φορά που χτυπάει το χαρτί.
    /// Αποθηκεύει ΠΑΝΤΑ την τελευταία απόφαση.
    /// </summary>
    public void SetStampDecision(VelocityStampTool.StampDecision decision)
    {
        hasBeenStamped = true;
        lastAppliedStamp = decision;
    }
}