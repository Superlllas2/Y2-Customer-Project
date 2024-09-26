using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public PlayerMovement PlayerMovement;
    private AudioManager audioManager;
    
    // private IEnumerator PlayStepSound()
    // {
    //     while (true)
    //     {
    //         audioManager.PlaySFX(audioManager.step);
    //         yield return new WaitForSeconds(stepInterval);
    //     }
    // }
    
    void Update()
    {
        // Corrected condition with parentheses
        if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)) && PlayerMovement.grounded)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // audioManager.PlaySFX(audioManager.jump);
            }
            else
            {
                // audioManager.PlaySFX(audioManager.step);
            }
        }
        else
        {

        }
    }

}
