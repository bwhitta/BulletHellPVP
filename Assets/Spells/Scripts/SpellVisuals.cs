using UnityEngine;

public static class SpellVisuals
{
    public static void EnableSprite(SpriteRenderer spriteRenderer, Sprite sprite, string maskLayer)
    {
        if (sprite == null)
        {
            Debug.LogWarning($"Sprite is null, cannot enable!");
            return;
        }

        // Display the sprite
        spriteRenderer.enabled = true;
        spriteRenderer.sprite = sprite;

        // Set the mask layer
        spriteRenderer.sortingLayerName = maskLayer;
    }
    public static void EnableAnimator(Animator animator, Transform transform, RuntimeAnimatorController animatorController, GameObject[] animationPrefabs, float animationScaleMultiplier, string maskLayer)
    {
        foreach (GameObject animationPrefab in animationPrefabs)
        {
            // Create the object to be animated
            GameObject animationObject = Object.Instantiate(animationPrefab, transform);
            animationObject.transform.localScale = new Vector2(animationScaleMultiplier, animationScaleMultiplier);

            // Animator does not work with changed names, so this line resets them.
            animationObject.name = animationPrefab.name;

            // Set the mask layer
            animationObject.GetComponent<SpriteRenderer>().sortingLayerName = maskLayer;
        }
        
        // Displays the animations
        animator.enabled = true;
        animator.runtimeAnimatorController = animatorController;
    }
    public static void EnableParticleSystem(Transform transform, GameObject particleSystemPrefab, string maskLayer)
    {
        GameObject particleObject = Object.Instantiate(particleSystemPrefab, transform);

        // Set the mask layer
        particleObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().sortingLayerName = maskLayer;
    }
    public static void StartingScale(Transform transform, float scale)
    {
        transform.localScale = new Vector3(scale, scale, 1);
    }
}
