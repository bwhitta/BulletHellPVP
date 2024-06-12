using UnityEngine;

public static class Calculations
{
    /// <summary>
    /// Finds the number that is across a specific percentage of the gap between two other numbers (by default finds the midpoint, 50% across).
    /// </summary>
    /// <param name="start">The number where the calculated point starts.</param>
    /// <param name="end">The number where the calculated number has reached 100% across.</param>
    /// <param name="percentageAcross">How far along the gap the calculated number should be. A value of 0 returns the start and a value of 1 returns the end.</param>
    public static float RelativeTo(float start, float end, float percentageAcross = 0.5f)
    {
        float distanceFromStart = (end - start) * percentageAcross;
        return distanceFromStart + start;
    }
    /// <summary>
    /// Finds the point that is across a specific percentage of the gap between two other points (by default finds the midpoint, 50% across).
    /// </summary>
    /// <param name="startPoint">The point where the calculated point starts.</param>
    /// <param name="endPoint">The point where the calculated point has reached 100% across.</param>
    /// <param name="percentageAcross">How far along the gap the calculated point should be. A value of 0 returns the start point and a value of 1 returns the end point.</param>
    public static Vector2 RelativeTo(Vector2 startPoint, Vector2 endPoint, float percentageAcross = 0.5f)
    {
        float x = RelativeTo(startPoint.x, endPoint.x, percentageAcross);
        float y = RelativeTo(startPoint.y, endPoint.y, percentageAcross);
        return new Vector2(x, y);
    }
    
    /// <summary>
    /// Calculates the remainder after dividing numberToModify by the modifyingNumber
    /// </summary>
    public static float Modulo(float numberToModify, float modifyingNumber)
    {
        return numberToModify - modifyingNumber * (Mathf.Floor(numberToModify / modifyingNumber));
    }

    /// <summary>
    /// Checks for a discrepancy between an existing and compared value. If the discrepancy is within the limit, it is unmodified, otherwise it becomes the compared value.
    /// </summary>
    public static float DiscrepancyCheck(float existingValue, float valueToCompare, float discrepancyLimit)
    {
        if (Mathf.Abs(existingValue - valueToCompare) > discrepancyLimit)
        {
            Debug.LogWarning($"Discrepancy of {Mathf.Abs(existingValue - valueToCompare)} detected, over limit of {discrepancyLimit}.");
            return valueToCompare;
        }
        else
        {
            return existingValue;
        }
    }

}