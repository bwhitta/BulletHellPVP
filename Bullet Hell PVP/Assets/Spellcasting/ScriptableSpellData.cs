using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Spell")]
public class ScriptableSpellData : ScriptableObject
{
    // Enums (add more as becomes necessary)

    /// <summary> The area modes that a spell can be cast in </summary>
    public enum ProjectileCastArea { NotApplicable, Single, Line}
    /// <summary> The types of movement currently available </summary>
    public enum ProjectileMovementType { NotApplicable, Linear, Bounce }
    /// <summary> The available targeting types </summary>
    public enum TargetType { NotApplicable, Player, Center, LinearOpposing, InvertedOpposing }

        [Header("General Info for Spell Manager")]
    /// <summary> The spell's name </summary>
    public string SpellName;
    /// <summary> The mana cost of the spell </summary>
    public float ManaCost;
    /// <summary> The cooldown after casting this spell </summary>
    public float Cooldown; // UNUSED

        [Header("Projectile info for Spell Manager")]
    /// <summary> How many times a projectile for the spell is created </summary>
    public int ProjectileQuantity;
    /// <summary> The shape in which the projectiles are instanciated </summary>
    public ProjectileCastArea ProjectileCastingArea; // UNUSED
    /// <summary> The scale multiplier of the casting area </summary>
    public float ProjectileCastingAreaScale; // UNUSED

    [Header("Info for Prefab")]
    /// <summary> How much the damage the spell deals </summary>
    public float Damage;
    /// <summary> How the spell moves </summary>
    public ProjectileMovementType MovementType;
    /// <summary> The speed multiplier of the spell's movement </summary>
    public float MovementSpeed;
    /// <summary> What the spell targets </summary>
    public TargetType TargetingType;

    [Header("Prefab")]
    /// <summary> The prefab object for the projectile. </summary>
    public GameObject ProjectilePrefab; // Should always use a simple prefab with SpellBehavior and a sprite renderer.
    /// <summary> The sprite for the projectile to use </summary>
    public Sprite ProjectileSprite;
}