using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Spell Scaling/Scale With Time")]
public class ScaleWithTime : SpellScaling
{
    public float ScalingDuration;
    public float EndScaleMultiplier;
    public AnimationCurve ScalingCurve;
    // could add a Delay float or something that adds an amount of time before the spell starts scaling

    public override float Scale(float distanceMoved, float spellLifespan)
    {
        // Caps at 100%
        float percentMoved = Mathf.Min(spellLifespan / ScalingDuration, 1f);

        float curveLength = ScalingCurve.keys[^1].time;
        float scaleCurveValue = ScalingCurve.Evaluate(percentMoved * curveLength);
        return Calculations.RelativeTo(1f, EndScaleMultiplier, scaleCurveValue);
    }
}
