using System;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class SpellModule
{
    public enum SpellTargets { Owner, Opponent }
    
    // Fields
    public int InstantiationQuantity;
    public SpellStartingPosition StartingPosition;
    public SpellStartingRotation StartingRotation;
    public float StartingScale;

    public SpellMovement[] SpellMovements;
    public SpellTargets SpellTarget;
    
    public SpellScaling[] SpellScalings;

    public bool PlayerAttached;

    public StatusEffect[] EffectsWhenAttached; // can have a seperate EffectsOnCollision
    /*public bool PushesPlayer;
    public float PlayerPushSpeed;*/

    /*public bool SpriteFacingPush;
    public bool AngleAfterStart;
    public float AngleChangeSpeed;*/

    public bool LimitedLifespan;
    public float Lifespan;

    public bool DestroyAfterDistanceMoved;
    public float DestroyDistance;

    public bool UsesCollider;
    public Vector2[] ColliderPath;

    public bool DealsDamage;
    public float Damage;

    public bool SpellUsesSprite;
    public Sprite UsedSprite;

    public bool UsesAnimation;
    public RuntimeAnimatorController AnimatorController;
    public GameObject[] AnimationPrefabs;
    public float AnimationScaleMultiplier;

    public bool GeneratesParticles;
    public GameObject ParticleSystemPrefab;
    
    // Value type that can be used to get a module (so that it can be sent through networking)
    public struct ModuleInfo : INetworkSerializeByMemcpy
    {
        // Constructors
        public ModuleInfo(byte setIndex, byte spellIndex, byte moduleIndex)
        {
            ModuleSpellData = new(setIndex, spellIndex);
            ModuleIndex = moduleIndex;
        }
        public ModuleInfo(SpellData.SpellInfo spellDataInfo, byte moduleIndex)
        {
            ModuleSpellData = spellDataInfo;
            ModuleIndex = moduleIndex;
        }

        // Fields
        public SpellData.SpellInfo ModuleSpellData;
        public byte ModuleIndex;

        // Properties
        public readonly SpellModule Module
        {
            get
            {
                return ModuleSpellData.Spell.UsedModules[ModuleIndex];
            }
        }
    }
}