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
    /// Καλείται από τον NPCController όταν η τελική απόφαση (για 1 ή 2 χαρτιά) είναι ΣΩΣΤΗ.
    /// </summary>
    public void AddScore()
    {
        currentScore++;
        Debug.Log($"<color=green>ΣΩΣΤΟ!</color> Σκορ: {currentScore}");
        UpdateScoreUI();
    }

    /// <summary>
    /// Καλείται από τον NPCController όταν η τελική απόφαση (για 1 ή 2 χαρτιά) είναι ΛΑΘΟΣ.
    /// </summary>
    public void SubtractScore()
    {
        currentScore--;
        Debug.Log($"<color=red>ΛΑΘΟΣ!</color> Σκορ: {currentScore}");
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