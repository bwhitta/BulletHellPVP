using UnityEngine;

public class SpellBehavior : MonoBehaviour
{
    public SpellData spellData;
    public int indexWithinSpell;
    public float distanceToMove;
    private float distanceMoved;

    // For target mode - character. 
    public GameObject targetedCharacter;

    private void Start()
    {
        if (spellData.SpellUsesSprite)
        {
            GetComponent<SpriteRenderer>().sprite = spellData.SpellSprite;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }


        gameObject.GetComponent<PolygonCollider2D>().enabled = spellData.SpellUsesCollider;
        SetCollider();
        
        AnimatorSetup();
        PointTowardsTarget();
    }

    private void SetCollider()
    {
        gameObject.GetComponent<PolygonCollider2D>().points = spellData.ColliderPath;
    }

    private void Update()
    {
        // Move based on movement
        MoveSpell();

        // Delete if too far away
        Vector2 currentPos = transform.position;
        float lengthFromCenter = Vector2.Distance(currentPos, Vector2.zero);
        if (lengthFromCenter >= 20)
        {
            Destroy(gameObject);
        }
    }

    private void PointTowardsTarget()
    {
        // If TargetingType is CharacterStats, point towards the character
        if (spellData.TargetingType == SpellData.TargetTypes.Character)
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
        else if (spellData.TargetingType == SpellData.TargetTypes.NotApplicable)
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
        if (spellData.MovementType == SpellData.MovementTypes.Linear)
        {
            transform.position += spellData.MovementSpeed * Time.deltaTime * transform.right;
            distanceMoved += spellData.MovementSpeed * Time.deltaTime;
        }
        else if (spellData.MovementType == SpellData.MovementTypes.Wall)
        {
            switch (indexWithinSpell)
            {
                case 0:
                    transform.position += transform.rotation * Vector3.up * Time.deltaTime * spellData.MovementSpeed;
                    break;
                case 1:
                    transform.position += transform.rotation * Vector3.down * Time.deltaTime * spellData.MovementSpeed;
                    break;
            }

            distanceMoved += Time.deltaTime * spellData.MovementSpeed;
        }

        // Scaling
        if(spellData.SpellScales)
            UpdateScaling();
    }
    private void UpdateScaling()
    {
        Debug.Log("Updating scaling");
        float distanceForScaling = distanceToMove * spellData.ScalingStartPercent;

        if (distanceMoved >= distanceForScaling)
        {
            float currentScale = Scaling(distanceToMove, spellData.ScalingStartPercent, distanceMoved, spellData.MaxScaleMultiplier - 1);
            transform.localScale = new Vector3(spellData.SpriteScale * currentScale, spellData.SpriteScale * currentScale, 1);
        }

        if (distanceMoved >= distanceToMove && spellData.DestroyOnScalingCompleted)
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
    private void AnimatorSetup()
    {
        // Enables the animator if AnimatedSpell is set to true
        gameObject.GetComponent<Animator>().enabled = spellData.AnimatedSpell;

        if (spellData.AnimatedSpell)
        {
            // Sets the animation
            gameObject.GetComponent<Animator>().runtimeAnimatorController = spellData.SpellAnimatorController;
        }
    }
}
