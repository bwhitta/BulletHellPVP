using UnityEngine;

public static class Calculations
{
    public static float RelativeTo(float start, float end, float percentageAcross = 0.5f)
    {
        float distanceFromStart = (end - start) * percentageAcross;
        return distanceFromStart + start;
    }
    public static Vector2 RelativeTo(Vector2 startPoint, Vector2 endPoint, float percentageAcross = 0.5f)
    {
        float x = RelativeTo(startPoint.x, endPoint.x, percentageAcross);
        float y = RelativeTo(startPoint.y, endPoint.y, percentageAcross);
        return new Vector2(x, y);
    }
    public static float Modulo(float numberToModify, float modifyingNumber)
    {
        return numberToModify - modifyingNumber * (Mathf.Floor(numberToModify / modifyingNumber));
    }
}
