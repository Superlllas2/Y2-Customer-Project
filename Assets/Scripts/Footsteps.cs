using System.Collections;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioManager audioManager;     // Reference to your AudioManager
    public float startDelay = 1f;         // Delay before sound starts

    private Coroutine footstepCoroutine;   // To track the active coroutine

    void Update()
    {
        // Check if any movement key is pressed
        if (IsMoving())
        {
            StartFootstepSound(); // Start playing footsteps if moving
        }
        else
        {
            StopFootstepSound(); // Stop footsteps if not moving
        }
    }

    private bool IsMoving()
    {
        // Check if any movement key is pressed
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
               Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }

    private void StartFootstepSound()
    {
        // Start the footstep sound coroutine if not already running
        if (footstepCoroutine == null)
        {
            footstepCoroutine = StartCoroutine(PlayStepSound());
        }
    }

    private void StopFootstepSound()
    {
        // Stop the coroutine if it's running
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null; // Reset the coroutine reference
            audioManager.StopSFX(); // Stop the sound immediately
        }
    }

    private IEnumerator PlayStepSound()
    {
        yield return new WaitForSeconds(startDelay); // Wait for the specified delay
        audioManager.PlaySFX(audioManager.step); // Start playing the walking sound

        // Keep playing until stopped
        while (IsMoving())
        {
            yield return null; // Wait for the next frame
        }
    }
}
