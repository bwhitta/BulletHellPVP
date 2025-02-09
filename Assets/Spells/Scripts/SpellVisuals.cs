using UnityEngine;

public class SpellVisuals : MonoBehaviour
{
    // Fields
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private SpellModuleBehavior spellModuleBehavior; // rename as soon as I rename the spellModuleBehavior script;

    // Properties
    SpellData.Module Module => spellModuleBehavior.Module;

    // Methods
    private void Start()
    {
        spellModuleBehavior = GetComponent<SpellModuleBehavior>();

        if (Module.SpellUsesSprite)
        {
            EnableSprite();
        }
        if (Module.Animated)
        {
            EnableAnimator();
        }
        if (Module.GeneratesParticles)
        {
            EnableParticleSystem();
        }

        StartingScale();
    }

    private void EnableSprite()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = Module.SpellUsesSprite;
        spriteRenderer.sprite = Module.UsedSprite;

        // Set the mask layer
        // string spellMaskLayer; REMOVED FOR RESTRUCTURING
        if (Module.ModuleType == SpellData.ModuleTypes.PlayerAttached)
        {
            // spellMaskLayer = OwnerCharacterInfo.SortingLayer; REMOVED FOR RESTRUCTURING
        }
        else
        {
            // spellMaskLayer = OwnerCharacterInfo.OpponentCharacterInfo.CharacterAndSortingTag; REMOVED FOR RESTRUCTURING
        }
        // spriteRenderer.sortingLayerName = spellMaskLayer; REMOVED FOR RESTRUCTURING
    }
    private void EnableAnimator()
    {
        animator = GetComponent<Animator>();
        foreach (GameObject animationPrefab in Module.MultipartAnimationPrefabs)
        {
            // Spawn in the animator
            GameObject currentAnimationPrefab = Instantiate(animationPrefab, transform);
            currentAnimationPrefab.transform.SetPositionAndRotation(transform.position, transform.rotation);
            currentAnimationPrefab.transform.localScale = new Vector2(Module.AnimationScaleMultiplier, Module.AnimationScaleMultiplier);

            // Animator does not work with changed name, so this line resets the name.
            currentAnimationPrefab.name = animationPrefab.name;

            // Make sure the sprite shows up on your own side of the play area when it is attached to yourself 
            // string spellMaskLayer; REMOVED FOR RESTRUCTURING
            if (Module.ModuleType == SpellData.ModuleTypes.PlayerAttached)
            {
                // spellMaskLayer = OwnerCharacterInfo.SortingLayer; REMOVED FOR RESTRUCTURING
            }
            else
            {
                // spellMaskLayer = OwnerCharacterInfo.OpponentCharacterInfo.CharacterAndSortingTag; REMOVED FOR RESTRUCTURING
            }

            // Set the mask layer
            // currentAnimationPrefab.GetComponent<SpriteRenderer>().sortingLayerName = spellMaskLayer; REMOVED FOR RESTRUCTURING
        }

        // Enables the animator if Animated is set to true
        animator.enabled = Module.Animated;

        // Sets the animation
        animator.runtimeAnimatorController = Module.AnimatorController;
    }
    private void EnableParticleSystem()
    {
        GameObject particleObject = Instantiate(Module.ParticleSystemPrefab, transform);
        particleObject.transform.localPosition = new Vector3(0, 0, Module.ParticleSystemZ);
    }

    private void StartingScale()
    {
        transform.localScale = new Vector3(Module.InstantiationScale, Module.InstantiationScale, 1);
    }
}
