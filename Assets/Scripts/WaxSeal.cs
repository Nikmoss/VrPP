using UnityEngine;

public class WaxSeal : MonoBehaviour
{
    [Tooltip("Το κεντρικό script της επιστολής (RoyalLetter)")]
    public SealedDocument parentDocument;

    private void OnTriggerEnter(Collider other)
    {
        DaggerTool dagger = other.GetComponentInParent<DaggerTool>();

        if (dagger != null)
        {
            if (parentDocument != null)
            {
                parentDocument.BreakSeal();
            }
        }
    }
}