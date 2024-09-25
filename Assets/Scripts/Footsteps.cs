using System.Collections;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioManager audioManager;     // Reference to your AudioManager
    public float startDelay = 1f;         // Delay before sound starts
    public float stepInterval = 0.5f;     // Interval between footstep sounds

    private Coroutine footstepCoroutine;   // To track the active footstep coroutine
    private Coroutine runningCoroutine;    // To track the active running coroutine

    void Update()
    {
        // Check if any movement key is pressed
        if (IsMoving())
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                StartRunningSound(); // Start running sound if holding shift
                StopFootstepSound(); // Ensure walking sound is not playing
            }
            else
            {
                StartFootstepSound(); // Start walking sound if not holding shift
                StopRunningSound();    // Ensure running sound is not playing
            }
        }
        else
        {
            StopFootstepSound(); // Stop footsteps if not moving
            StopRunningSound();   // Stop running sound if not moving
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
            audioManager.StopSFX(); // Stop the walking sound immediately
        }
    }

    private IEnumerator PlayStepSound()
    {
        yield return new WaitForSeconds(startDelay); // Wait for the specified delay

        // Play the walking sound at intervals while moving
        while (IsMoving() && !Input.GetKey(KeyCode.LeftShift)) // Stop if shift is pressed
        {
            audioManager.PlaySFX(audioManager.step); // Start playing the walking sound
            yield return new WaitForSeconds(stepInterval); // Wait for the step interval
        }
    }

    private void StartRunningSound()
    {
        // Start the running sound coroutine if not already running
        if (runningCoroutine == null)
        {
            runningCoroutine = StartCoroutine(PlayRunningSound());
        }
    }

    private void StopRunningSound()
    {
        // Stop the coroutine if it's running
        if (runningCoroutine != null)
        {
            StopCoroutine(runningCoroutine);
            runningCoroutine = null; // Reset the coroutine reference
            audioManager.StopSFX(); // Stop the running sound immediately
        }
    }

    private IEnumerator PlayRunningSound()
    {
        yield return new WaitForSeconds(startDelay); // Wait for the specified delay

        // Play the running sound at intervals while running
        while (IsMoving() && Input.GetKey(KeyCode.LeftShift)) // Continue while shift is pressed
        {
            audioManager.PlaySFX(audioManager.run); // Start playing the running sound
            yield return new WaitForSeconds(stepInterval); // Wait for the step interval
        }
    }
}
