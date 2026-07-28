using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Προσθήκη για τα νέα XR namespaces αν χρειάζεται

[RequireComponent(typeof(XRGrabInteractable))]
public class SealedDocument : MonoBehaviour
{
    [Header("Κατάσταση Επιστολής")]
    public bool isSealed = true;
    public bool isOpen = false;

    [Header("Οπτικά Αντικείμενα (Μοντέλα)")]
    public GameObject sealedEnvelopeModel;
    public GameObject unsealedEnvelopeModel;
    public GameObject openedLetterModel;

    [Header("Εφέ Σπασίματος")]
    public AudioSource breakSealSound;
    public ParticleSystem waxShatterParticles;

    [Header("Περιεχόμενο (UI)")]
    public TMP_Text letterText;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnTriggerPressed);
        }

        UpdateVisuals();
        GenerateRoyalDecree();
    }

    private void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnTriggerPressed);
        }
    }

    public void BreakSeal()
    {
        if (!isSealed) return;

        isSealed = false;

        if (breakSealSound != null) breakSealSound.Play();
        if (waxShatterParticles != null) waxShatterParticles.Play();

        UpdateVisuals();
    }

    private void OnTriggerPressed(ActivateEventArgs args)
    {
        if (isSealed) return;

        isOpen = !isOpen;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (sealedEnvelopeModel != null)
            sealedEnvelopeModel.SetActive(isSealed);

        if (unsealedEnvelopeModel != null)
            unsealedEnvelopeModel.SetActive(!isSealed && !isOpen);

        if (openedLetterModel != null)
            openedLetterModel.SetActive(!isSealed && isOpen);
    }

    private void GenerateRoyalDecree()
    {
        if (letterText != null)
        {
            letterText.text = "ΑΥΣΤΗΡΗ ΔΙΑΤΑΓΗ\n\nΕκ μέρους του Λόρδου Διοικητή.\n\nΟι έμποροι που μεταφέρουν ΟΠΛΑ πρέπει να ελέγχονται διπλά. Απορρίψτε οποιονδήποτε έχει ληγμένο πάσο, χωρίς εξαιρέσεις.\n\nΥπογραφή:\nΤο Συμβούλιο";
        }
    }
}