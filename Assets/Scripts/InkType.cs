using UnityEngine;

/// <summary>
/// Μπαίνει σε έναν αόρατο Trigger Collider πάνω από τη θήκη του μελανιού.
/// </summary>
public class InkPadZone : MonoBehaviour
{
    [Tooltip("Τι είδους μελάνι δίνει αυτή η θήκη;")]
    public VelocityStampTool.StampDecision inkType;

    private void OnTriggerEnter(Collider other)
    {
        // Ψάχνουμε αν το αντικείμενο που μπήκε στο Trigger είναι η σφραγίδα (ή μέρος της)
        VelocityStampTool stamp = other.GetComponentInParent<VelocityStampTool>();

        if (stamp != null)
        {
            stamp.DipInInk(inkType);
        }
    }
}