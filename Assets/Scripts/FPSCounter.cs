using UnityEngine;
using TMPro;

/// <summary>
/// Υπολογίζει και εμφανίζει τα FPS στην οθόνη/VR. 
/// Ξεκλειδώνει το framerate και αλλάζει χρώμα ανάλογα με την απόδοση.
/// </summary>
public class FPSCounter : MonoBehaviour
{
    [Tooltip("Το Text (TMP) που θα δείχνει τα FPS")]
    public TMP_Text fpsText;

    private float deltaTime = 0.0f;

    private void Update()
    {
        // Υπολογισμός του χρόνου μεταξύ των frames
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        // Μετατροπή σε Frames Per Second
        float fps = 1.0f / deltaTime;

        if (fpsText != null)
        {
            fpsText.text = $"FPS: {Mathf.Ceil(fps)}";

            // Οπτική ένδειξη της απόδοσης του VR
            if (fps >= 72)
            {
                fpsText.color = Color.green; // Τέλεια απόδοση
            }
            else if (fps >= 45)
            {
                fpsText.color = Color.yellow; // Προσοχή, πέφτουν τα frames 
            }
            else
            {
                fpsText.color = Color.red; // Κίνδυνος για motion sickness!
            }
        }
    }
}