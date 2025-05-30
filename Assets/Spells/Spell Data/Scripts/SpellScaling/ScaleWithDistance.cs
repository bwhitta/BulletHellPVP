using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spell Scaling/Scale With Distance")]
public class ScaleWithDistance : SpellScaling
{
    public float ScalingDistance;
    public float EndScaleMultiplier;
    public AnimationCurve ScalingCurve;

    public override float Scale(float distanceMoved, float spellLifespan)
    {
        // Caps at 100%
        float percentMoved = Mathf.Min(distanceMoved / ScalingDistance, 1f);

        float curveLength = ScalingCurve.keys[^1].time;
        float scaleCurveValue = ScalingCurve.Evaluate(percentMoved * curveLength);
        return Calculations.RelativeTo(1f, EndScaleMultiplier, scaleCurveValue);
    }
}
