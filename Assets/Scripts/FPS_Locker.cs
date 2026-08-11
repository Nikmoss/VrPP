using UnityEngine;

public class FPSLocker : MonoBehaviour
{
    private void Awake()
    {
        // Λέει στο Unity να τρέχει αυστηρά στα 120 FPS
        Application.targetFrameRate = 120;
    }
}