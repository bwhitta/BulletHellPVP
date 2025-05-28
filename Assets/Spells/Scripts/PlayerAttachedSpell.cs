using UnityEngine;

public class PlayerAttachedSpell : MonoBehaviour
{
    /*private void FixedUpdate()
    {
        // Make sprite face towards where the character is being pushed
        if (Module.SpriteFacingPush)
        {
            var angle = Vector2.SignedAngle(Vector2.up, tempPlayerMovementMod);
            transform.rotation = Quaternion.Euler(0, 0, 180 + angle);
        }
        
        // Local Methods
        if (Module.AngleAfterStart)
        {
            TryAnglingPush();
        }
        
        // what does this even do:
        float GetAngle(Vector2 vector)
        {
            // Returns angle from top, counterclockwise
            return Vector2.SignedAngle(Vector2.up, vector);
        }
    }*/

    /* REMOVED FOR RESTRUCTURING, when re-implementing this will need some extra work done to make the inputs sync with the server
    private void TryAnglingPush()
    {
        Vector2 inputVector = characterControls.movementAction.ReadValue<Vector2>();
        float movingDirection = GetAngle(tempPlayerMovementMod.tempPush);
        float inputDirection = GetAngle(inputVector);
        if (inputVector == Vector2.zero)
            return;
        float movementCap = Module.AngleChangeSpeed * Time.fixedDeltaTime;
        float rotationAngle = Mathf.MoveTowardsAngle(movingDirection, inputDirection, movementCap);
        // Change the push direction to still move the player 
        tempPlayerMovementMod.tempPush = Quaternion.Euler(0, 0, rotationAngle) * Vector2.up;
    }*/
}
