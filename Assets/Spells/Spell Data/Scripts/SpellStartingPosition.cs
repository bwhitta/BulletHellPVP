using System;
using UnityEngine;

[Serializable]
public class SpellStartingPosition
{
    // could intead have this as a abstract class, inheriting GetPosition for each different spawning type.
    // this would probably be best if this script start having multiple fields that are irellevant to certain types or if this script starts getting too long.

    public enum PositionTypes { Point, Cursor, AdjacentCorners }

    // Fields
    public PositionTypes PositionType;
    public Vector2 Offset;

    // Methods
    public Vector2 GetPosition(byte moduleObjectIndex, float cursorLocation, byte targetId)
    {
        return PositionType switch
        {
            PositionTypes.Point => PointStartingPosition(),
            PositionTypes.Cursor => CursorStartingPosition(cursorLocation, targetId),
            PositionTypes.AdjacentCorners => AdjacentCornersStartingPosition(moduleObjectIndex, cursorLocation, targetId),
            _ => throw new NotImplementedException("Starting position type not found"),
        };
    }

    private Vector2 PointStartingPosition()
    {
        return Offset;
    }
    private Vector2 CursorStartingPosition(float cursorLocation, byte targetId)
    {
        Vector2 battleAreaCenter = GameSettings.Used.BattleAreaCenters[(int)targetId];
        return CursorMovement.CalculateCursorPosition(cursorLocation, battleAreaCenter) + Offset;
    }
    private Vector2 AdjacentCornersStartingPosition(byte moduleObjectIndex, float cursorLocation, byte targetId)
    {
        int side = Calculations.SquareSideAtPosition(GameSettings.Used.BattleSquareWidth, cursorLocation);
        Vector2[] corners = Calculations.GetSquareCorners(GameSettings.Used.BattleSquareWidth, GameSettings.Used.BattleAreaCenters[targetId]);

        // Points to instantiate at
        Vector2[] spawnPoints = new Vector2[]
        {
                    corners[side],
                    corners[(side + 1) % 4]
        };

        return spawnPoints[moduleObjectIndex] + Offset;
    }

    /* use if I decide to turn this back into an abstract class:
    public override Vector2 GetPosition(SpellModule module, byte moduleObjectindex, float cursorLocation, byte targetId)
    {
        Vector2 battleAreaCenter = GameSettings.Used.BattleAreaCenters[(int)targetId];
        return CursorMovement.CalculateCursorPosition(cursorLocation, battleAreaCenter) + Offset;
    }*/
}
