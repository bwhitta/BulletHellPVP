using UnityEngine;

public class PlayerAttachedSpell : MonoBehaviour
{
    // Fields
    private SpellModuleBehavior spellModuleBehavior; // rename as soon as I rename the spellModuleBehavior script;

    // Properties
    SpellData.Module Module => spellModuleBehavior.Module;
    
    private void Start()
    {
        spellModuleBehavior = GetComponent<SpellModuleBehavior>();

        // Attach module (if applicable)
        if (Module.ModuleType == SpellData.ModuleTypes.PlayerAttached)
        {
            // Set up parenting
            //transform.parent = OwnerCharacterInfo.CharacterObject.transform; REMOVED FOR RESTRUCTURING
            //transform.localPosition = Vector2.zero; REMOVED FOR RESTRUCTURING

            // Set how long the spell should last
            // attachmentTime = Module.AttachmentTime; REMOVED FOR RESTRUCTURING
        }
    }

    private void FixedUpdate()
    {
        /*
        attachmentTime--;
        if (attachmentTime <= 0)
        {
            DestroySelfNetworkSafe();
        }

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
        } REMOVED FOR RESTRUCTURING */
    }

    /* private void TryAnglingPush()
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
    } REMOVED FOR RESTRUCTURING, when re-implementing this will need some extra work done to make the inputs sync with the server*/
}
