using UnityEngine;

public static class SpellVisuals
{
    /* REMOVED FOR RESTRUCTURING
    private void EnableSprite()
    {
        Debug.Log($"enabling sprite! sprite is null: {Module.UsedSprite == null}");

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.enabled = Module.SpellUsesSprite;
        spriteRenderer.sprite = Module.UsedSprite;

        // Set the mask layer
        if (Module.ModuleType == SpellModule.ModuleTypes.PlayerAttached)
        {
            spriteRenderer.sortingLayerName = GameSettings.Used.spellMaskLayers[spellInfoLogic.OwnerId];
        }
        else
        {
            spriteRenderer.sortingLayerName = GameSettings.Used.spellMaskLayers[spellInfoLogic.OpponentId];
        }
    }*/
    /* REMOVED FOR RESTRUCTURING
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
            string spellMaskLayer;
            if (Module.ModuleType == SpellModule.ModuleTypes.PlayerAttached)
            {
                spellMaskLayer = GameSettings.Used.spellMaskLayers[spellInfoLogic.OwnerId];
            }
            else
            {
                spellMaskLayer = GameSettings.Used.spellMaskLayers[spellInfoLogic.OpponentId];
            }

            // Set the mask layer
            animationObject.GetComponent<SpriteRenderer>().sortingLayerName = spellMaskLayer;
        }

        // Enables the animator if Animated is set to true
        animator.enabled = Module.UsesAnimation;

        // Sets the animation
        animator.runtimeAnimatorController = Module.AnimatorController;
    } */
    /* REMOVED FOR RESTRUCTURING
    private void EnableParticleSystem()
    {
        GameObject particleObject = Instantiate(Module.ParticleSystemPrefab, transform);
        particleObject.transform.localPosition = new Vector3(0, 0, Module.ParticleSystemZ);
    }*/
    /* REMOVED FOR RESTRUCTURING
    private void StartingScale()
    {
        transform.localScale = new Vector3(Module.InstantiationScale, Module.InstantiationScale, 1);
    }*/
}
