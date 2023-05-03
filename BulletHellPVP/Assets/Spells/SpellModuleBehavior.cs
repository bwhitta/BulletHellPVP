using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class SpellModuleBehavior : MonoBehaviour
{
    public SpellData.Module module;
    public int spellBehaviorID;
    
    // Projectile
    public float distanceToMove;
    private float distanceMoved;
    public GameObject targetedCharacter;

    // Player Attached
    private float attachmentTime;
    private CharacterControls characterControls;
    private Vector2 movementDirection;

    // Display
    public string spellMaskLayer;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Readonlys
    private readonly float outOfBoundsDistance = 15f;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = new Vector3(module.InstantiationScale, module.InstantiationScale, 1);

        spriteRenderer.enabled = module.UsesSprite;
        if (module.UsesSprite)
            EnableSprite();
        if (module.Animated)
            EnableAnimator();
        if (module.GeneratesParticles)
            EnableParticleSystem();

        switch (module.ModuleType)
        {
            case SpellData.ModuleTypes.PlayerAttached:
                attachmentTime = module.AttachmentTime;
                characterControls = transform.parent.GetComponent<CharacterControls>();
                if (module.PushesPlayer)
                {
                    movementDirection = characterControls.movementAction.ReadValue<Vector2>().normalized;
                    // If the player is stationary, send down
                    if(movementDirection == Vector2.zero)
                    {
                        movementDirection = new Vector2(0, -1);
                    }
                }
                break;
        }

        gameObject.GetComponent<PolygonCollider2D>().enabled = module.UsesCollider;
        SetCollider();
        PointTowardsTarget();

        #region LocalMethods
        void EnableAnimator()
        {
            animator = gameObject.GetComponent<Animator>();
            foreach (GameObject animationPrefab in module.MultipartAnimationPrefabs)
            {
                GameObject currentAnimationPrefab = Instantiate(animationPrefab, transform);

                currentAnimationPrefab.transform.SetPositionAndRotation(transform.position, transform.rotation);
                currentAnimationPrefab.transform.localScale = new Vector2(module.AnimationScaleMultiplier, module.AnimationScaleMultiplier);
                // Animator does not work with changed name, so this line resets the name.
                currentAnimationPrefab.name = animationPrefab.name;

                // Set the mask layer
                currentAnimationPrefab.GetComponent<SpriteRenderer>().sortingLayerName = spellMaskLayer;
            }

            // Enables the animator if Animated is set to true
            animator.enabled = module.Animated;

            // Sets the animation
            animator.runtimeAnimatorController = module.AnimatorController;
        }
        void EnableSprite()
        {
            spriteRenderer.sprite = module.Sprite;
            // Set the mask layer
            spriteRenderer.sortingLayerName = spellMaskLayer;
        }
        void EnableParticleSystem()
        {
            GameObject particleObject = Instantiate(module.ParticleSystemPrefab, transform);
            particleObject.transform.localPosition = new Vector3(0, 0, module.ParticleSystemZ);
        }
        #endregion LocalMethods
    }

    private void SetCollider()
    {
        gameObject.GetComponent<PolygonCollider2D>().points = module.ColliderPath;
    }

    private void Update()
    {
        switch (module.ModuleType)
        {
            case SpellData.ModuleTypes.Projectile:
                MoveSpell();
                break;
            case SpellData.ModuleTypes.PlayerAttached:
                PlayerAttachedUpdate();
                break;
        }

        // Delete if too far away
        CheckBounds();

        // Local Methods
        void CheckBounds()
        {
            float distanceFromCenter = Vector2.Distance(transform.position, Vector2.zero);
            if (distanceFromCenter >= outOfBoundsDistance)
            {
                Destroy(gameObject);
            }
        }
        
    }
    private void PlayerAttachedUpdate()
    {
        TryPushPlayer();
        TryAffectPlayerMovement();

        // Attatchment Time
        attachmentTime -= Time.deltaTime;
        if (attachmentTime <= 0)
        {
            Destroy(gameObject);
        }

        #region PlayerAttachedLocalMethods
        void TryPushPlayer()
        {
            if (module.PushesPlayer)
            {
                characterControls.tempPush += module.PlayerPushSpeed * movementDirection;
                TryAnglingPush();
            }
            if (module.SpriteFacingPush)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180 + GetAngle(movementDirection));
            }
            //transform.right = targetedCharacter.transform.position
        }
        void TryAffectPlayerMovement()
        {
            if (module.AffectsPlayerMovement)
            {
                characterControls.tempMovementMod = module.PlayerMovementMod;
            }
        }
        void TryAnglingPush()
        {
            if (module.AngleAfterStart)
            {
                Vector2 inputVector = characterControls.movementAction.ReadValue<Vector2>();
                float movingDirection = GetAngle(movementDirection);
                float inputDirection = GetAngle(inputVector);
                if (inputVector == Vector2.zero)
                    return;
                float movementCap = module.AngleChangeSpeed * Time.deltaTime;
                float rotationAngle = Mathf.MoveTowardsAngle(movingDirection, inputDirection, movementCap);
                movementDirection = Quaternion.Euler(0, 0, rotationAngle) * Vector2.up;
            }
            
        }
        float GetAngle(Vector2 vector)
        {
            // Returns angle from top, counterclockwise
            return Vector2.SignedAngle(Vector2.up, vector);
        }
        #endregion
    }


    private void PointTowardsTarget()
    {
        // If TargetingType is CharacterStats, point towards the character
        if (module.TargetingType == SpellData.TargetTypes.Character)
        {
            if (targetedCharacter == null)
            {
                Debug.LogWarning("Targeted character assigned as null");
            }
            else
            {
                transform.right = targetedCharacter.transform.position - transform.position; // Point towards character
            }
        }
        else if (module.TargetingType == SpellData.TargetTypes.NotApplicable)
        {
            //Do nothing
            return;
        }
        else
        {
            Debug.LogWarning("Targeting type is not yet implemented.");
        }
    }
    private void MoveSpell()
    {
        // Move the spell
        if (module.MovementType == SpellData.MovementTypes.Linear)
        {
            transform.position += module.MovementSpeed * Time.deltaTime * transform.right;
            distanceMoved += module.MovementSpeed * Time.deltaTime;
        }
        else if (module.MovementType == SpellData.MovementTypes.Wall)
        {
            switch (spellBehaviorID)
            {
                case 0:
                    transform.position += transform.rotation * Vector3.up * Time.deltaTime * module.MovementSpeed;
                    break;
                case 1:
                    transform.position += transform.rotation * Vector3.down * Time.deltaTime * module.MovementSpeed;
                    break;
            }

            distanceMoved += Time.deltaTime * module.MovementSpeed;
        }

        // Scaling
        if(module.ScalesOverTime)
            UpdateScaling();
    }
    private void UpdateScaling()
    {
        float distanceForScaling = distanceToMove * module.ScalingStartPercent;

        if (distanceMoved >= distanceForScaling)
        {
            float currentScale = Scaling(distanceToMove, module.ScalingStartPercent, distanceMoved, module.MaxScaleMultiplier - 1);
            transform.localScale = new Vector3(module.InstantiationScale * currentScale, module.InstantiationScale * currentScale, 1);
        }

        if (distanceMoved >= distanceToMove && module.DestroyOnScalingCompleted)
            Destroy(gameObject);
    }
    private float Scaling(float totalMove, float totalMoveScalingStartPercent, float currentlyMoved, float scaleTargetPercentage)
    {
        // The position along totalMove at which scaling starts
        float scalingStart = totalMove * totalMoveScalingStartPercent;
        // The percentage (0.0 to 1.0) of scaling completed
        float scalingCompletionPercentage = (currentlyMoved - scalingStart) / (totalMove - scalingStart);
        // Cap at 1.0 (100%)
        scalingCompletionPercentage = Mathf.Min(scalingCompletionPercentage, 1f);

        return (scaleTargetPercentage * scalingCompletionPercentage) + 1f;
    }
}
