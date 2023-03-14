using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Spell")]
public class ScriptableSpellData : ScriptableObject
{
    /// <summary> The area modes that a spell can be cast in </summary>
    public enum CastArea { NotApplicable, Single, Stacked, AdjacentCorners }
    /// <summary> The types of movement currently available </summary>
    public enum SpellType { NotApplicable, Linear, Wall }
    /// <summary> The available targeting types </summary>
    public enum TargetType { NotApplicable, Character, Center, LinearOpposing, InvertedOpposing }

        [Space(25)][Header("Info for Spell Manager")]
    public string SpellName;
    public float ManaCost;
    public float SpellCooldown;
    public int ProjectileQuantity;
    public CastArea CastingArea;
    public float CastingAreaScale; // UNUSED

        [Space(25)][Header("Info for Prefab")]
    public float Damage;
    public SpellType TypeOfSpell;
    public float MovementSpeed;
    public TargetType TargetingType;

        [Space(25)][Header("Prefab")]
    public GameObject ProjectilePrefab;
    public bool SpellUsesSprite;
    public Sprite ProjectileSprite;
    public float SpriteScale;

        [Space(25)][Header("UI")]
    public Sprite Icon;
        
        [Space(25)][Header("Optional (only used for some spell types and such)")]
    public float SecondaryCastingArea;
    [Space] // Animation
    public bool AnimateSpell;
    public RuntimeAnimatorController SpellAnimatorController;
    public GameObject[] MultipartAnimationPrefabs;
    [Space] // Scaling
    public bool ScalingAfterDistance;
    public float ScalingStart;
    public bool DestroyOnScalingCompleted;

        [Space(25)][Header("Collision Info")]
    public bool UsesCollider;
    public Vector2[] ColliderPath;

        [Space(25)][Header("Selection Info")]
    public string description;
}