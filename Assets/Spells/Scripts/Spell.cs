using UnityEngine;

public class Spell : MonoBehaviour
{
    // Fields
    [HideInInspector] public SpellModule Module;
    [HideInInspector] public byte ModuleObjectIndex;
    [HideInInspector] public byte? TargetId;
    
    void Start()
    {
        // Set up visuals
        if (Module.SpellUsesSprite)
        {
            //SpellVisuals.EnableSprite();
        }
        if (Module.UsesAnimation)
        {
            //SpellVisuals.EnableAnimator();
        }
        if (Module.GeneratesParticles)
        {
            //SpellVisuals.EnableParticleSystem();
        }
        //SpellVisuals.StartingScale();
    }

    void Update()
    {
        // go through SpellMovement list and run their move method. either make inherited SOs like SpellMovementWaves and SpellMovementHoming, or have the mode determined by an enum
    }

    
}
