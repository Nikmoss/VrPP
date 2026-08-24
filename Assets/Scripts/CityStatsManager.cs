using System.Collections;
using UnityEngine;

public class CityStatsManager : MonoBehaviour
{
    // ==========================================
    // 1. ΤΑ 4 ΣΤΑΤΙΣΤΙΚΑ ΤΗΣ ΠΟΛΗΣ
    // ==========================================
    [Header("Τρέχοντα Στατιστικά (Max 100)")]
    [Range(0, 100)] public int security = 50;
    [Range(0, 100)] public int morale = 50;
    [Range(0, 100)] public int food = 50;
    [Range(0, 100)] public int health = 50;

    [Header("Ήχοι Άβακα")]
    public AudioSource beadMoveSound;       // Ήχος χάντρας
    public AudioSource rouletteTickSound;   // Γρήγορο τικ (ρουλέτα)
    public AudioSource errorRevealSound;    // Κλακ (τελικό σύμβολο ρουλέτας)
    public AudioSource carvingSound;        // Ήχος σκαλίσματος στο ξύλο (για νέο κανόνα)

    // ==========================================
    // 2. ΥΠΟΜΝΗΜΑ / ΣΚΑΛΙΣΜΑΤΑ (Πάνω μέρος)
    // ==========================================
    [Header("Σκαλίσματα Υπομνήματος (Top Legend)")]
    public GameObject legendExpired;     // Κλεψύδρα (Ληγμένο)
    public GameObject legendWrongName;   // Πρόσωπο (Λάθος Όνομα)
    public GameObject legendWrongEmblem; // Ασπίδα (Λάθος Έμβλημα)
    public GameObject legendBanned;      // Κρανίο/Πύργος (Απαγορευμένη Πόλη/Είδος)

    // ==========================================
    // 3. ΜΗΧΑΝΙΣΜΟΣ ΡΟΥΛΕΤΑΣ (Κάτω μέρος)
    // ==========================================
    [Header("Οθόνη Ρουλέτας (Στη βάση)")]
    [Tooltip("Βάλε εδώ το Sprite που βρίσκεται στο κάτω μέρος του Άβακα")]
    public SpriteRenderer errorDisplayRenderer;

    [Header("Σύμβολα Λαθών (Sprites για Ρουλέτα)")]
    public Sprite iconBlank;
    public Sprite iconExpired;
    public Sprite iconWrongEmblem;
    public Sprite iconWrongName;
    public Sprite iconBanned;

    [Header("Ρυθμίσεις Ρουλέτας")]
    public float displayDuration = 6.0f; // Πόσο θα μείνει ανοιχτό το τελικό σύμβολο

    public enum ErrorType { None, Expired, WrongEmblem, WrongName, BannedRule }

    private void Start()
    {
        // Αρχικά, η ρουλέτα είναι κενή και τα σκαλίσματα κρυμμένα
        if (errorDisplayRenderer != null) errorDisplayRenderer.sprite = iconBlank;

        if (legendExpired != null) legendExpired.SetActive(false);
        if (legendWrongName != null) legendWrongName.SetActive(false);
        if (legendWrongEmblem != null) legendWrongEmblem.SetActive(false);
        if (legendBanned != null) legendBanned.SetActive(false);
    }

    // ==========================================
    // ΛΕΙΤΟΥΡΓΙΕΣ STATS ΚΑΙ ΡΟΥΛΕΤΑΣ
    // ==========================================

    // Αλλάζει τα Stats και (αν υπάρχει ήχος) ακούγεται η χάντρα
    public void ModifyStats(int secChange, int morChange, int foodChange, int healthChange)
    {
        security = Mathf.Clamp(security + secChange, 0, 100);
        morale = Mathf.Clamp(morale + morChange, 0, 100);
        food = Mathf.Clamp(food + foodChange, 0, 100);
        health = Mathf.Clamp(health + healthChange, 0, 100);

        if (beadMoveSound != null && (secChange != 0 || morChange != 0 || foodChange != 0 || healthChange != 0))
        {
            beadMoveSound.Play();
        }
    }

    // Καλείται από τον NPCController όταν γίνεται λάθος
    public void ReportError(ErrorType type)
    {
        Debug.Log("<color=yellow>Η Ρουλέτα κλήθηκε για το λάθος: </color>" + type.ToString());
        ModifyStats(-5, 0, 0, 0);
        StartCoroutine(RouletteRoutine(type));
    }

    private IEnumerator RouletteRoutine(ErrorType finalType)
    {
        Sprite[] allIcons = { iconExpired, iconWrongEmblem, iconWrongName, iconBanned };
        Sprite finalSprite = iconBlank;

        // Βρίσκουμε ποιο είναι το τελικό σύμβολο που πρέπει να "κάτσει"
        switch (finalType)
        {
            case ErrorType.Expired: finalSprite = iconExpired; break;
            case ErrorType.WrongEmblem: finalSprite = iconWrongEmblem; break;
            case ErrorType.WrongName: finalSprite = iconWrongName; break;
            case ErrorType.BannedRule: finalSprite = iconBanned; break;
        }

        int spins = Random.Range(12, 18);
        float spinDelay = 0.04f;

        // Το γύρισμα της ρουλέτας
        for (int i = 0; i < spins; i++)
        {
            if (errorDisplayRenderer != null)
            {
                errorDisplayRenderer.sprite = allIcons[Random.Range(0, allIcons.Length)];
            }
            if (rouletteTickSound != null) rouletteTickSound.Play();

            yield return new WaitForSeconds(spinDelay);
            spinDelay += 0.015f; // Επιβράδυνση
        }

        // Το τελικό αποτέλεσμα
        if (errorDisplayRenderer != null) errorDisplayRenderer.sprite = finalSprite;
        if (errorRevealSound != null) errorRevealSound.Play();

        // Περιμένουμε για να το δει ο παίκτης
        yield return new WaitForSeconds(displayDuration);

        // Το κρύβουμε ξανά
        if (errorDisplayRenderer != null) errorDisplayRenderer.sprite = iconBlank;
    }

    // ==========================================
    // ΕΜΦΑΝΙΣΗ ΝΕΟΥ ΚΑΝΟΝΑ (ΣΚΑΛΙΣΜΑ) ΣΤΗΝ ΑΡΧΗ ΤΗΣ ΜΕΡΑΣ
    // ==========================================
    public void RevealNewLegendRule(ErrorType newRuleType)
    {
        if (carvingSound != null) carvingSound.Play();

        switch (newRuleType)
        {
            case ErrorType.Expired: if (legendExpired != null) legendExpired.SetActive(true); break;
            case ErrorType.WrongName: if (legendWrongName != null) legendWrongName.SetActive(true); break;
            case ErrorType.WrongEmblem: if (legendWrongEmblem != null) legendWrongEmblem.SetActive(true); break;
            case ErrorType.BannedRule: if (legendBanned != null) legendBanned.SetActive(true); break;
        }
    }
}