using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class DaySettings
{
    [Header("Γενικά")]
    public string dayName = "Day 1";
    [Tooltip("Το τρέχον έτος μέσα στο παιχνίδι για αυτή τη μέρα")]
    public int currentGameYear = 2026;

    [Header("Κανόνες Ημέρας")]
    [Tooltip("Η πόλη που απαγορεύεται σήμερα (κενό αν δεν υπάρχει)")]
    public string bannedCity = "";
    [Tooltip("Το αντικείμενο που απαγορεύεται σήμερα (κενό αν δεν υπάρχει)")]
    public string bannedItem = "";

    [Header("Νέοι Κανόνες για τον Άβακα")]
    [Tooltip("Ποια νέα σύμβολα θα 'χαραχτούν' σήμερα στο πάνω μέρος του Άβακα;")]
    public List<CityStatsManager.ErrorType> newRulesToCarveToday = new List<CityStatsManager.ErrorType>();

    [Header("Κείμενο Γράμματος")]
    [TextArea(5, 10)]
    public string letterContent = "Ministry of Admission\n\nDirectives for today:\n- Standard protocol.";

    [Header("Εξαφάνιση Εργαλείων (Τικ = Κρύβεται)")]
    [Tooltip("Τσέκαρε το κουτάκι για να ΕΞΑΦΑΝΙΣΕΙΣ το αντικείμενο αυτή τη μέρα.")]
    public bool hideStamp = true;
    public bool hideKnife = true;
    public bool hidePen = true;
    public bool hideGong = true;
    public bool hideGongMallet = true;
    public bool hideRulebook = true;

    [Header("Πιθανότητες Εμφάνισης (Spawns)")]
    [Range(0f, 1f)] public float merchantSpawnProbability = 0.3f;
    [Range(0f, 1f)] public float mercenarySpawnProbability = 0.1f;

    [Header("Δυσκολία - Πιθανότητες Πλαστογραφίας")]
    [Range(0f, 1f)] public float citizenForgeryProbability = 0.0f;
    [Range(0f, 1f)] public float merchantForgeryProbability = 0.0f;

    [Header("Κατανομή Λαθών Πολίτη")]
    [Range(0f, 1f)]
    [Tooltip("Πιθανότητα το λάθος (αν είναι πλαστό) να είναι Ληγμένο Διαβατήριο. Το υπόλοιπο θα είναι Λάθος Έμβλημα.")]
    public float expiredErrorChance = 0.75f;

    [Header("Κατανομή Λαθών Εμπόρου (Βάρη / Weights)")]
    public float weightPassportError = 20f;
    public float weightNameError = 20f;
    public float weightCityError = 20f;
    public float weightDateError = 20f;
    public float weightPurposeError = 20f;
    public float weightPermitEmblemError = 20f;
}

public class DayManager : MonoBehaviour
{
    public static DayManager Instance { get; private set; }

    [Header("Αναφορές UI")]
    public TMP_Text letterText;

    [Header("Αναφορές Εργαλείων (GameObjects)")]
    public GameObject stampObject;
    public GameObject knifeObject;
    public GameObject gongObject;
    public GameObject penObject;
    public GameObject gongMalletObject;
    public GameObject rulebookObject;

    [Header("Ρυθμίσεις Ημερών")]
    public int currentDayIndex = 0;
    public List<DaySettings> days = new List<DaySettings>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ApplyDaySettings();
    }

    public void ApplyDaySettings()
    {
        if (days.Count == 0 || currentDayIndex >= days.Count)
        {
            Debug.LogWarning("Δεν έχουν ρυθμιστεί άλλες μέρες στον DayManager!");
            return;
        }

        DaySettings today = days[currentDayIndex];

        if (letterText != null)
        {
            letterText.text = today.letterContent;
        }

        // --- ΕΝΕΡΓΟΠΟΙΗΣΗ / ΑΠΕΝΕΡΓΟΠΟΙΗΣΗ ΕΡΓΑΛΕΙΩΝ ---
        if (stampObject != null) stampObject.SetActive(!today.hideStamp);
        if (penObject != null) penObject.SetActive(!today.hidePen);
        if (knifeObject != null) knifeObject.SetActive(!today.hideKnife);
        if (gongObject != null) gongObject.SetActive(!today.hideGong);
        if (gongMalletObject != null) gongMalletObject.SetActive(!today.hideGongMallet);
        if (rulebookObject != null) rulebookObject.SetActive(!today.hideRulebook);

        // --- ΧΑΡΑΞΗ ΝΕΩΝ ΣΥΜΒΟΛΩΝ ΣΤΟΝ ΑΒΑΚΑ ---
        CityStatsManager cityStats = FindObjectOfType<CityStatsManager>();
        if (cityStats != null && today.newRulesToCarveToday != null)
        {
            foreach (var rule in today.newRulesToCarveToday)
            {
                cityStats.RevealNewLegendRule(rule);
            }
        }
    }

    public void NextDay()
    {
        if (currentDayIndex < days.Count - 1)
        {
            currentDayIndex++;
            ApplyDaySettings();
        }
        else
        {
            Debug.Log("Τέλος Παιχνιδιού ή δεν υπάρχουν άλλες μέρες!");
        }
    }

    public DaySettings GetCurrentDaySettings()
    {
        if (days.Count > 0 && currentDayIndex < days.Count)
            return days[currentDayIndex];
        return null;
    }
}