using UnityEngine;
using TMPro;

/// <summary>
/// Υπολογίζει και εμφανίζει τα FPS στην οθόνη/VR. 
/// Ανανεώνει την ένδειξη με καθυστέρηση για πιο εύκολη ανάγνωση στο VR.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    [Header("Αναφορές")]
    [Tooltip("Το Text (TMP) που θα δείχνει τα FPS")]
    public TMP_Text fpsText;

    [Header("Ρυθμίσεις Ανανέωσης")]
    [Tooltip("Κάθε πόσα δευτερόλεπτα θα ανανεώνεται το νούμερο στην οθόνη.")]
    public float updateInterval = 0.5f;

    private float accumulator = 0f;
    private int frames = 0;
    private float timeLeft;

    private void Start()
    {
        // Εξαναγκασμός του Unity να στοχεύσει τα 120 FPS
        Application.targetFrameRate = 120;
        timeLeft = updateInterval;
    }

    private void Update()
    {
        timeLeft -= Time.unscaledDeltaTime;
        accumulator += Time.unscaledDeltaTime;
        frames++;

        // Ανανέωση του κειμένου μόνο όταν περάσει ο χρόνος (π.χ. κάθε 0.5 δευτερόλεπτα)
        if (timeLeft <= 0.0f)
        {
            float fps = frames / accumulator;

            if (fpsText != null)
            {
                fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";

                // Οπτική ένδειξη της απόδοσης ειδικά για τα 120Hz του PSVR2
                if (fps >= 110f)
                {
                    fpsText.color = Color.green; // Τέλεια απόδοση
                }
                else if (fps >= 72f)
                {
                    fpsText.color = Color.yellow; // Προσοχή, πέφτουν τα frames 
                }
                else
                {
                    fpsText.color = Color.red; // Κίνδυνος για motion sickness!
                }
            }

            // Επαναφορά μετρητών για τον επόμενο κύκλο
            timeLeft = updateInterval;
            accumulator = 0.0f;
            frames = 0;
        }
    }
}