using System;
using UnityEngine;

[Serializable]
public class SpellStartingRotation
{
    public enum RotationTypes { ExactAngle, TowardsTarget, CursorDirection }

    // Fields
    public RotationTypes PositionType;
    public float Offset;

    // Methods
    public Quaternion GetRotation(Vector2 startingPosition, float cursorLocation, byte targetId)
    {
        return PositionType switch
        {
            RotationTypes.ExactAngle => ExactAngleRotation(),
            RotationTypes.TowardsTarget => TowardsTargetRotation(startingPosition, targetId),
            RotationTypes.CursorDirection => CursorDirectionRotation(cursorLocation),
            _ => throw new NotImplementedException("Starting position type not found"),
        };
    }

    private Quaternion ExactAngleRotation()
    {
        return Quaternion.Euler(0, 0, Offset);
    }
    private Quaternion TowardsTargetRotation(Vector2 startingPosition, byte targetId)
    {
        Vector2 difference = (Vector2)CharacterManager.CharacterTransforms[targetId].position - startingPosition;

        float angle = Vector2.SignedAngle(Vector2.up, difference);
        
        return Quaternion.Euler(0, 0, angle + Offset);
    }
    private Quaternion CursorDirectionRotation(float cursorLocation)
    {
        float angle = CursorMovement.CalculateCursorRotation(cursorLocation).eulerAngles.z;
        return Quaternion.Euler(0, 0, angle + Offset);
    }
}
