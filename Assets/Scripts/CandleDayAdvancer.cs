using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class CandleDayAdvancer : MonoBehaviour
{
    [Header("Οπτικά Κεριού")]
    [Tooltip("Το GameObject που περιέχει τα Particles της φωτιάς")]
    public GameObject flameParticles;
    [Tooltip("Το Φως που ρίχνει το κερί στο γραφείο")]
    public Light candleLight;

    [Header("Ρυθμίσεις Μετάβασης")]
    [Tooltip("Πόσα δευτερόλεπτα θα μείνει σβηστό το κερί (και το δωμάτιο σκοτεινό)")]
    public float transitionDelay = 3.0f;

    private XRBaseInteractable interactable;
    private bool isTransitioning = false;

    private void Awake()
    {
        // Πλέον βρίσκει αυτόματα το XR Grab Interactable που βάλαμε
        interactable = GetComponent<XRBaseInteractable>();
    }

    private void OnEnable()
    {
        interactable.selectEntered.AddListener(OnFlameGrabbed);
    }

    private void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnFlameGrabbed);
    }

    private void OnFlameGrabbed(SelectEnterEventArgs args)
    {
        // Αν ήδη αλλάζουμε μέρα, αγνόησε τα πολλαπλά πατήματα
        if (!isTransitioning)
        {
            StartCoroutine(DayTransitionRoutine());
        }
    }

    private IEnumerator DayTransitionRoutine()
    {
        isTransitioning = true;

        // 1. ΣΒΗΣΙΜΟ: Κλείνουμε τη φλόγα και το φως
        if (flameParticles != null) flameParticles.SetActive(false);
        if (candleLight != null) candleLight.enabled = false;

        // 2. ΑΝΑΜΟΝΗ: Το δωμάτιο μένει στο σκοτάδι
        yield return new WaitForSeconds(transitionDelay);

        // 3. ΕΝΗΜΕΡΩΣΗ ΣΥΣΤΗΜΑΤΩΝ: Πάμε στην επόμενη μέρα και μηδενίζουμε το σκορ
        if (DayManager.Instance != null)
        {
            DayManager.Instance.NextDay();
        }

        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.ResetDailyStats();
        }

        // --- ΝΕΟ: Ειδοποιούμε τον Spawner να διαγράψει NPC και χαρτιά ---
        NPCSpawner spawner = FindObjectOfType<NPCSpawner>();
        if (spawner != null)
        {
            spawner.ResetSpawner();
        }

        // 4. ΑΝΑΜΜΑ: Ανάβουμε ξανά τη φλόγα
        if (flameParticles != null) flameParticles.SetActive(true);
        if (candleLight != null) candleLight.enabled = true;

        isTransitioning = false;
    }
}