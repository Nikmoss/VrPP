using UnityEngine;
using TMPro;

/// <summary>
/// Δημιουργεί τυχαία στοιχεία διαβατηρίου (ονόματα, ημερομηνίες), 
/// ενημερώνει το Canvas και "θυμάται" την τελευταία σφραγίδα που δέχτηκε.
/// </summary>
public class DynamicPassport : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Το TextMeshPro που δείχνει τα στοιχεία του διαβατηρίου.")]
    public TMP_Text passportText;

    [Header("Ρυθμίσεις Παιχνιδιού")]
    public int currentGameYear = 1982;
    [Range(0f, 1f)] public float expirationProbability = 0.3f;

    [Header("Κατάσταση (Διαβάζεται από άλλα scripts)")]
    public bool isExpired;
    public string currentFirstName;
    public string currentLastName;

    [Header("Κατάσταση Σφραγίδας")]
    public bool hasBeenStamped = false;
    public VelocityStampTool.StampDecision lastAppliedStamp;

    private readonly string[] firstNames = { "GREGOR", "IVAN", "ANNA", "MARIA", "DMITRI", "ELENA", "BORIS", "NATALIA", "YURI", "KATYA" };
    private readonly string[] lastNames = { "IVANOV", "SMIRNOV", "POPOV", "SOKOLOV", "VOLKOV", "KOZLOV", "MOROZOV", "NOVIKOV", "PETROV" };

    private void Start()
    {
        GenerateData();
    }

    private void GenerateData()
    {
        currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
        currentLastName = lastNames[Random.Range(0, lastNames.Length)];

        isExpired = Random.value < expirationProbability;

        int expirationYear;
        int expirationMonth = Random.Range(1, 13);
        int expirationDay = Random.Range(1, 29);

        if (isExpired)
        {
            expirationYear = Random.Range(currentGameYear - 5, currentGameYear);
        }
        else
        {
            expirationYear = Random.Range(currentGameYear, currentGameYear + 10);
        }

        string formattedDate = $"{expirationDay:00}-{expirationMonth:00}-{expirationYear}";

        if (passportText != null)
        {
            passportText.text = $"ΟΝΟΜΑ: {currentFirstName}\nΕΠΙΘΕΤΟ: {currentLastName}\nΗΜ. ΛΗΞΗΣ: {formattedDate}";
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