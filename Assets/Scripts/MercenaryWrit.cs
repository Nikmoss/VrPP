using UnityEngine;
using UnityEngine.UI; // Απαραίτητο για να αλλάζουμε χρώμα στο UI Image
using TMPro;

/// <summary>
/// Το στρατιωτικό έγγραφο (Mercenary Writ).
/// Έχει πιθανότητα να είναι ΠΛΑΣΤΟ (είτε με ορθογραφικό λάθος στη φατρία, 
/// είτε με ΛΑΘΟΣ χρώμα σφραγίδας).
/// </summary>
public class MercenaryWrit : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Το TextMeshPro που δείχνει όλα τα στοιχεία του εγγράφου.")]
    public TMP_Text writText;

    [Tooltip("Το UI Image που αναπαριστά την προ-τυπωμένη σφραγίδα της φατρίας.")]
    public Image factionSealImage;

    [Header("Ρυθμίσεις Δυσκολίας")]
    [Tooltip("Πιθανότητα το έγγραφο να είναι πλαστό (π.χ. 0.4 = 40%)")]
    [Range(0f, 1f)] public float forgeryProbability = 0.4f;

    [Header("Κατάσταση (Διαβάζεται από άλλα scripts)")]
    public string currentSoldierName;
    public string currentFactionName;
    public bool isForged;
    public string forgeryReason = ""; // Για να ξέρουμε γιατί ήταν πλαστό

    [Header("Κατάσταση Ελέγχου")]
    public bool hasBeenStamped = false;
    public VelocityStampTool.StampDecision lastAppliedStamp = VelocityStampTool.StampDecision.None;

    // Δεδομένα για τυχαία παραγωγή
    private readonly string[] soldierNames = { "GERALT", "BRONN", "ARTHUR", "LANCELOT", "GARETH", "CLEGANE", "BARRISTAN", "DUNCAN", "JORAH" };

    // Σωστά ονόματα φατριών
    private readonly string[] validFactions = { "NORTH VANGUARD", "IRON GUILD", "ROYAL GUARD", "SILVER SWORDS", "BLACK RAVENS", "RED SHIELDS" };

    // Πλαστά (ανορθόγραφα) ονόματα φατριών
    private readonly string[] forgedFactions = { "NORT VANGUARD", "IRON GULD", "ROYAL GAURD", "SILVER SOWRDS", "BLAK RAVENS", "RED SHEILDS" };

    // Τα σωστά χρώματα για την κάθε φατρία (πρέπει να έχουν την ίδια σειρά με τα ονόματα από πάνω)
    private readonly Color[] factionColors = {
        Color.blue,                 // North Vanguard
        Color.gray,                 // Iron Guild
        Color.red,                  // Royal Guard
        Color.white,                // Silver Swords
        Color.black,                // Black Ravens
        new Color(0.6f, 0f, 0f)     // Red Shields (Σκούρο Κόκκινο)
    };

    private void Awake()
    {
        GenerateData();
    }

    private void GenerateData()
    {
        // 1. Επιλέγουμε τυχαίο όνομα και φατρία
        currentSoldierName = soldierNames[Random.Range(0, soldierNames.Length)];
        int factionIndex = Random.Range(0, validFactions.Length);

        currentFactionName = validFactions[factionIndex];
        Color assignedColor = factionColors[factionIndex];

        // 2. Ρίχνουμε το ζάρι της πλαστογραφίας
        isForged = Random.value < forgeryProbability;

        if (isForged)
        {
            // Αν είναι πλαστό, διαλέγουμε ΤΙ είδους πλαστογραφία θα είναι (50-50 πιθανότητα)
            bool isTypoForgery = Random.value < 0.5f;

            if (isTypoForgery)
            {
                // Πλαστογραφία 1: Ορθογραφικό λάθος στο όνομα
                currentFactionName = forgedFactions[factionIndex];
                forgeryReason = "Ορθογραφικό Λάθος (Typo)";
            }
            else
            {
                // Πλαστογραφία 2: Λάθος χρώμα σφραγίδας
                int wrongColorIndex = Random.Range(0, factionColors.Length);
                // Βεβαιωνόμαστε ότι το λάθος χρώμα δεν είναι κατά τύχη το σωστό
                while (wrongColorIndex == factionIndex)
                {
                    wrongColorIndex = Random.Range(0, factionColors.Length);
                }
                assignedColor = factionColors[wrongColorIndex];
                forgeryReason = "Λάθος Χρώμα Σφραγίδας";
            }
        }

        // 3. Ενημέρωση του UI
        if (writText != null)
        {
            writText.text = $"ΣΤΡΑΤΙΩΤΙΚΟ ΕΓΓΡΑΦΟ\n\nΟΝΟΜΑ: {currentSoldierName}\nΦΑΤΡΙΑ: {currentFactionName}";
        }

        if (factionSealImage != null)
        {
            factionSealImage.color = assignedColor;
        }
    }

    // Η Σφραγίδα ή η Πένα καλούν αυτή τη μέθοδο
    public void SetStampDecision(VelocityStampTool.StampDecision decision)
    {
        if (hasBeenStamped) return;

        hasBeenStamped = true;
        lastAppliedStamp = decision;

        EvaluatePlayerDecision(decision);

        NPCController npc = FindObjectOfType<NPCController>();
        if (npc != null) npc.DocumentWasStamped();
    }

    private void EvaluatePlayerDecision(VelocityStampTool.StampDecision playerDecision)
    {
        if (isForged)
        {
            if (playerDecision == VelocityStampTool.StampDecision.Denied)
            {
                Debug.Log($"<color=green>ΣΩΣΤΗ ΑΠΟΦΑΣΗ!</color> Ο παίκτης εντόπισε την πλαστογραφία ({forgeryReason}) και τον απέρριψε.");
            }
            else
            {
                Debug.Log($"<color=red>ΛΑΘΟΣ ΑΠΟΦΑΣΗ (STRIKE)!</color> Ο παίκτης υπέγραψε πλαστό έγγραφο! Αιτία: {forgeryReason}");
            }
        }
        else
        {
            if (playerDecision == VelocityStampTool.StampDecision.Approved)
            {
                Debug.Log("<color=green>ΣΩΣΤΗ ΑΠΟΦΑΣΗ!</color> Ο παίκτης υπέγραψε σωστά τον γνήσιο στρατιώτη.");
            }
            else
            {
                Debug.Log("<color=red>ΛΑΘΟΣ ΑΠΟΦΑΣΗ (STRIKE)!</color> Ο παίκτης απέρριψε άδικα έναν γνήσιο στρατιώτη!");
            }
        }
    }
}