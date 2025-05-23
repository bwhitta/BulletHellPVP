using System;
using UnityEngine;

[Serializable]
public class SpellStartingRotation
{
    // could intead have this as a abstract class, inheriting GetPosition for each different spawning type.
    // this would probably be best if this script start having multiple fields that are irellevant to certain types or if this script starts getting too long.

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
        float angle = Vector2.Angle(startingPosition, CharacterManager.CharacterTransforms[targetId].position);
        return Quaternion.Euler(0, 0, angle + Offset);
    }
    private Quaternion CursorDirectionRotation(float cursorLocation)
    {
        int side = Calculations.SquareSideAtPosition(GameSettings.Used.BattleSquareWidth, cursorLocation);
        float angle = -90 * side;
        return Quaternion.Euler(0, 0, angle + Offset);
    }
}
