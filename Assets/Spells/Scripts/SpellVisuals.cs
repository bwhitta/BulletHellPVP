using UnityEngine;

public class SpellVisuals : MonoBehaviour
{
    // Fields
    [SerializeField] private string[] spellMaskLayers;

    private Animator animator;

    private SpellInfoLogic spellModuleBehavior; // rename as soon as I rename the spellModuleBehavior script;

    // Properties
    SpellModule Module => spellModuleBehavior.Module;

    // Methods
    private void Start()
    {
        spellModuleBehavior = GetComponent<SpellInfoLogic>();

        if (Module.SpellUsesSprite)
        {
            EnableSprite();
        }
        if (Module.UsesAnimation)
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
        Debug.Log($"enabling sprite! sprite is null: {Module.UsedSprite == null}");

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.enabled = Module.SpellUsesSprite;
        spriteRenderer.sprite = Module.UsedSprite;

        // Set the mask layer
        if (Module.ModuleType == SpellModule.ModuleTypes.PlayerAttached)
        {
            spriteRenderer.sortingLayerName = spellMaskLayers[spellModuleBehavior.OwnerId];
        }
        else
        {
            spriteRenderer.sortingLayerName = spellMaskLayers[spellModuleBehavior.OpponentId];
        }
    }
    private void EnableAnimator()
    {
        animator = GetComponent<Animator>();
        foreach (GameObject animationPrefab in Module.MultipartAnimationPrefabs)
        {
            // Spawn in the animator
            GameObject animationObject = Instantiate(animationPrefab, transform);
            animationObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
            animationObject.transform.localScale = new Vector2(Module.AnimationScaleMultiplier, Module.AnimationScaleMultiplier);

            // Animator does not work with changed name, so this line resets the name.
            animationObject.name = animationPrefab.name;

            // Make sure the sprite shows up on your own side of the play area when it is attached to yourself 
            // string spellMaskLayer; REMOVED FOR RESTRUCTURING
            if (Module.ModuleType == SpellModule.ModuleTypes.PlayerAttached)
            {
                // spellMaskLayer = OwnerCharacterInfo.SortingLayer; REMOVED FOR RESTRUCTURING
            }
            else
            {
                // spellMaskLayer = OwnerCharacterInfo.OpponentCharacterInfo.CharacterAndSortingTag; REMOVED FOR RESTRUCTURING
            }

            // Set the mask layer
            animationObject.GetComponent<SpriteRenderer>().sortingLayerName = spellMaskLayers[spellModuleBehavior.OwnerId];
        }

        // Enables the animator if Animated is set to true
        animator.enabled = Module.UsesAnimation;

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
