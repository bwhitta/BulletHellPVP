using System;
using UnityEngine;

[Serializable]
public class BasicStats: ScriptableObject
{
    [Header("Movement")]
    public float MovementSpeedMod;

    [Space]
    [Header("Health")]
    public float MaxHealthStat;

    [Space]
    [Header("Mana")]
    public float StartingMaxMana;
    public float EndingMaxMana;
    public float ScalingTime;
    public float StartingManaRegen;
    public float EndingManaRegen;

    [Space]
    [Header("Health/Mana Bar Visuals")]
    public float StatLostVelocityMod;

    [Space]
    [Header("Damage Invincibility")]
    public float InvincibilityTime;
    public float InvincibilityAlphaMod;

    /* (UNUSED) Mana regen on damage
    [Space]
    public float RegenOnDamageMod;
    public float ManaDamageRegenTime;*/
}