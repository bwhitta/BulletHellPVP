using System;
using UnityEngine;

[Serializable]
public class BasicStats: ScriptableObject
{
    public float InvincibilityTime;
    public float InvincibilityAlphaMod;
    [Space]
    public float MaxHealthStat;
    public float MaxManaStat;
    [Space]
    public float BaseManaRegen;
    [Space]
    public float MovementSpeedMod;
    [Space]
    public float StatLostVelocityMod;
}