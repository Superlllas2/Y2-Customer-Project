using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public PlayerMovement PlayerMovement;

    void Update()
    {
        // Placeholder for step SFX driven by Input System move/sprint.
        if (GameInput.Instance == null || PlayerMovement == null || !PlayerMovement.grounded)
        {
            return;
        }

        if (GameInput.Instance.MoveValue.sqrMagnitude <= 0.01f)
        {
            return;
        }

        // audioManager.PlaySFX(GameInput.Instance.SprintHeld ? audioManager.jump : audioManager.step);
    }
}
