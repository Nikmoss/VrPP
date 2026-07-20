using UnityEngine;
using TMPro; // Απαραίτητο για το κείμενο UI

/// <summary>
/// Διαχειρίζεται το σκορ του παίκτη και ανανεώνει το χαρτάκι στο γραφείο.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    [Header("Ρυθμίσεις Σκορ")]
    [Tooltip("Το 3D Κείμενο (TextMeshPro) που θα δείχνει το σκορ πάνω στο γραφείο.")]
    public TMP_Text scoreText;

    private int currentScore = 0;

    private void Start()
    {
        // Ενημερώνουμε το χαρτάκι μόλις ξεκινήσει το παιχνίδι
        UpdateScoreUI();
    }

    /// <summary>
    /// Καλείται από τη σφραγίδα μόλις χτυπήσει το έγγραφο.
    /// </summary>
    public void EvaluateDecision(VelocityStampTool.StampDecision decision, DynamicPassport passport)
    {
        bool isCorrect = false;

        // ΛΟΓΙΚΗ 1: Εγκρίθηκε (Approved) και το διαβατήριο ΔΕΝ έχει λήξει
        if (decision == VelocityStampTool.StampDecision.Approved && !passport.isExpired)
        {
            isCorrect = true;
        }
        // ΛΟΓΙΚΗ 2: Απορρίφθηκε (Denied) και το διαβατήριο ΕΙΧΕ λήξει
        else if (decision == VelocityStampTool.StampDecision.Denied && passport.isExpired)
        {
            isCorrect = true;
        }

        // Εφαρμογή πόντων
        if (isCorrect)
        {
            currentScore++;
            Debug.Log($"<color=green>ΣΩΣΤΟ!</color> Σκορ: {currentScore}");
        }
        else
        {
            currentScore--;
            Debug.Log($"<color=red>ΛΑΘΟΣ!</color> Σκορ: {currentScore}");
        }

        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"ΣΚΟΡ: {currentScore}";
        }
    }
}