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
        return numberToModify - (modifyingNumber * Mathf.Floor(numberToModify / modifyingNumber));
    }

    /// <summary>
    /// Checks for a discrepancy between an existing and a compared float. If the discrepancy is within the limit, it is unmodified, otherwise it becomes the compared value.
    /// </summary>
    public static float DiscrepancyCheck(float existingValue, float valueToCompare, float discrepancyLimit)
    {
        if (Mathf.Abs(existingValue - valueToCompare) > discrepancyLimit)
        {
            Debug.LogWarning($"Discrepancy of {Mathf.Abs(existingValue - valueToCompare)} (existingValue: {existingValue}, valueToCompare: {valueToCompare}) detected, over limit of {discrepancyLimit}.");
            return valueToCompare;
        }
        else
        {
            return existingValue;
        }
    }
    /// <summary>
    /// Checks for a discrepancy between an existing and a compared Vector2. If the discrepancy is within the limit, it is unmodified, otherwise it becomes the compared value.
    /// </summary>
    public static Vector2 DiscrepancyCheck(Vector2 existingValue, Vector2 valueToCompare, float discrepancyLimit)
    {
        Vector2 discrepancyVector = existingValue - valueToCompare;
        if (discrepancyVector.magnitude > discrepancyLimit)
        {
            Debug.LogWarning($"Discrepancy of {discrepancyVector.magnitude} (existingValue: {existingValue}, valueToCompare: {valueToCompare}) detected, over limit of {discrepancyLimit}.");
            return valueToCompare;
        }
        else
        {
            return existingValue;
        }
    }

    /// <summary>
    /// Gets the positions of the corners of a square.
    /// </summary>
    /// <returns>The positions of the corners, starting from the top left and continuing clockwise</returns>
    public static Vector2[] GetSquareCorners(float sideLength, Vector2 centerPoint)
    {
        float offset = sideLength/2;

        Vector2[] corners = new Vector2[4];
        corners[0] = centerPoint + new Vector2(-offset, offset);
        corners[1] = centerPoint + new Vector2(offset, offset);
        corners[2] = centerPoint + new Vector2(offset, -offset);
        corners[3] = centerPoint + new Vector2(-offset, -offset);

        return corners;
    }

    /// <summary>
    /// Gets which side a point is on the edge of a square, starting from the top left and continuing clockwise.
    /// </summary>
    public static int SquareSideAtPosition(float squareWidth, float locationAroundSquare)
    {
        int sideNumber = (int)Mathf.Floor(Modulo(locationAroundSquare, squareWidth * 4f) / squareWidth);

        // Floating points can create a bug when the value is a really small negative number (e.g. -1e^-8). If that happens, this will fix it.
        if (sideNumber >= 4) sideNumber = 0;

        return sideNumber;
    }

    public static T[] Populate<T>(this T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
        return array;
    }
}