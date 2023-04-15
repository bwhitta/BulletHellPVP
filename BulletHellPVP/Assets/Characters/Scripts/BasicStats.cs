using System;
using UnityEngine;

[Serializable]
public class BasicStats: ScriptableObject
{
    public float InvincibilityTime;
    public float InvincibilityAlphaMod;
    [Space]
    public float MaxHealthStat;
    [Space]
    public float ScalingTime;
    public float StartingMaxMana;
    public float EndingMaxMana;
    [Space]
    public float StartingManaRegen;
    public float EndingManaRegen;
    [Space]
    public float MovementSpeedMod;
    [Space]
    public float StatLostVelocityMod;

    /* (UNUSED) Mana regen on damage
    [Space]
    public float RegenOnDamageMod;
    public float ManaDamageRegenTime;*/
}