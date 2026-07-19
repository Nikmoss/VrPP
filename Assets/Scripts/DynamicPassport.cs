using UnityEngine;
using TMPro; // Απαραίτητο για να μιλήσουμε στο TextMeshPro

/// <summary>
/// Δημιουργεί τυχαία στοιχεία διαβατηρίου (ονόματα, ημερομηνίες) 
/// και ενημερώνει το Canvas πάνω στο 3D αντικείμενο.
/// </summary>
public class DynamicPassport : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Το TextMeshPro που δείχνει τα στοιχεία του διαβατηρίου.")]
    public TMP_Text passportText;

    [Header("Ρυθμίσεις Παιχνιδιού")]
    [Tooltip("Η τρέχουσα χρονιά μέσα στο παιχνίδι (π.χ. 1982).")]
    public int currentGameYear = 1982;

    [Tooltip("Πιθανότητα (0 έως 1) το διαβατήριο να είναι ληγμένο. Το 0.3 σημαίνει 30%.")]
    [Range(0f, 1f)]
    public float expirationProbability = 0.3f;

    [Header("Κατάσταση (Διαβάζεται από άλλα scripts)")]
    public bool isExpired;
    public string currentFirstName;
    public string currentLastName;

    // Πίνακες με τυχαία ονόματα για ποικιλία
    private readonly string[] firstNames = { "BILLARAS", "AGELOS", "GIORGOS", "NIKOLAS", "DMITRIS", "FILIP", "SWTOS", "XRISTOFOROS", "PATRICK", "ALIKI" };
    private readonly string[] lastNames = { "SPANDOS", "KOUP", "PAPPOUS", "TEOFAS", "BIBOS", "MPISK", "TSAP", "MOSS", "KOUTR" };

    private void Start()
    {
        // Μόλις το διαβατήριο δημιουργηθεί (γίνει Instantiate από τον NPC), 
        // φτιάχνει κατευθείαν τα νέα του στοιχεία.
        GenerateData();
    }

    private void GenerateData()
    {
        // 1. Τυχαία επιλογή ονόματος
        currentFirstName = firstNames[Random.Range(0, firstNames.Length)];
        currentLastName = lastNames[Random.Range(0, lastNames.Length)];

        // 2. Υπολογισμός Λήξης
        isExpired = Random.value < expirationProbability;

        int expirationYear;
        int expirationMonth = Random.Range(1, 13);
        int expirationDay = Random.Range(1, 29); // 1 έως 28 για να αποφύγουμε θέματα με Φεβρουάριο

        if (isExpired)
        {
            // Αν είναι ληγμένο, η χρονιά λήξης είναι στο παρελθόν
            expirationYear = Random.Range(currentGameYear - 5, currentGameYear);
        }
        else
        {
            // Αν είναι έγκυρο, η χρονιά λήξης είναι στο μέλλον ή ακριβώς φέτος
            expirationYear = Random.Range(currentGameYear, currentGameYear + 10);

            // Αν έτυχε η φετινή χρονιά, σιγουρευόμαστε ότι ο μήνας/μέρα είναι στο μέλλον
            // (Για απλοποίηση στο demo, αν πέσει φέτος, το θεωρούμε έγκυρο)
        }

        string formattedDate = $"{expirationDay:00}-{expirationMonth:00}-{expirationYear}";

        // 3. Ενημέρωση του 3D Κειμένου
        if (passportText != null)
        {
            passportText.text = $"ΟΝΟΜΑ: {currentFirstName}\nΕΠΙΘΕΤΟ: {currentLastName}\nΗΜ. ΛΗΞΗΣ: {formattedDate}";
        }
        else
        {
            Debug.LogError("<color=red>ΣΦΑΛΜΑ:</color> Δεν έχεις συνδέσει το Text (TMP) στο script DynamicPassport!");
        }
    }
}