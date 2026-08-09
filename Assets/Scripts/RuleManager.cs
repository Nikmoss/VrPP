using UnityEngine;
using TMPro;

/// <summary>
/// Διαχειρίζεται τους καθημερινούς κανόνες και περιορισμούς του παιχνιδιού.
/// Εμφανίζει τους κανόνες σε ένα απλό UI Text (όπως το FPS Counter).
/// </summary>
public class RuleManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Το Text (TMP) που θα δείχνει τους σημερινούς κανόνες")]
    public TMP_Text ruleText;

    [Header("Κατάσταση Παιχνιδιού")]
    public int currentDay = 1;

    // Αυτές οι μεταβλητές είναι public (κρυμμένες όμως από τον Inspector) 
    // για να μπορεί να τις διαβάζει το NPCController αργότερα.
    [HideInInspector] public string bannedCity = "";
    [HideInInspector] public string bannedItem = "";

    // Λίστες όμοιες με τα έγγραφα (Εξαιρούμε τον Volos από τα Ban γιατί είναι η πόλη μας)
    private readonly string[] cities = { "ATHENS", "SPARTA", "THEBES", "CORINTH" };
    private readonly string[] items = { "Salt (Barrels)", "Spices (Sacks)", "Olive Oil (Jars)", "Copper Ingots", "Silver Coins" };

    private void Start()
    {
        // Μόλις ξεκινήσει το παιχνίδι, δημιουργούμε τους κανόνες της ημέρας
        GenerateRulesForDay(currentDay);
    }

    /// <summary>
    /// Καλείται για να δημιουργήσει κανόνες για μια συγκεκριμένη μέρα.
    /// Μπορείς να την καλέσεις από ένα κουμπί "Next Day".
    /// </summary>
    public void GenerateRulesForDay(int day)
    {
        currentDay = day;

        // Μηδενίζουμε τους κανόνες της προηγούμενης μέρας
        bannedCity = "";
        bannedItem = "";

        if (currentDay == 1)
        {
            // Ημέρα 1: Καμία απαγόρευση (Εκμάθηση)
        }
        else
        {
            // Από την Ημέρα 2 και μετά, ρίχνουμε "ζάρι"
            float randomVal = Random.value;

            if (randomVal < 0.33f)
            {
                // Απαγορεύεται μόνο μια πόλη
                bannedCity = cities[Random.Range(0, cities.Length)];
            }
            else if (randomVal < 0.66f)
            {
                // Απαγορεύεται μόνο ένα εμπόρευμα
                bannedItem = items[Random.Range(0, items.Length)];
            }
            else
            {
                // Δύσκολη μέρα: Απαγορεύεται ΚΑΙ μια πόλη ΚΑΙ ένα εμπόρευμα
                bannedCity = cities[Random.Range(0, cities.Length)];
                bannedItem = items[Random.Range(0, items.Length)];
            }
        }

        UpdateUI();
    }

    /// <summary>
    /// Προχωράει το παιχνίδι στην επόμενη μέρα.
    /// </summary>
    public void NextDay()
    {
        GenerateRulesForDay(currentDay + 1);
    }

    private void UpdateUI()
    {
        if (ruleText == null) return;

        string rules = $"<color=#FFD700>--- DAY {currentDay} ---</color>\n\n";
        rules += "<b>Official Directives:</b>\n";

        if (bannedCity == "" && bannedItem == "")
        {
            rules += "- Open borders. No current restrictions.\n";
        }
        else
        {
            if (bannedCity != "")
                rules += $"- Entry from <color=red>{bannedCity}</color> is DENIED.\n";

            if (bannedItem != "")
                rules += $"- Trade of <color=red>{bannedItem}</color> is CONTRABAND.\n";
        }

        ruleText.text = rules;
    }
}