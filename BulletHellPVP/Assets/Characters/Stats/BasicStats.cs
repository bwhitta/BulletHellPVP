using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
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