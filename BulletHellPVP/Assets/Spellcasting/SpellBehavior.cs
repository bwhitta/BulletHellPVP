using System;
using Unity.VisualScripting;
using UnityEngine;

public class SpellBehavior : MonoBehaviour
{
    public ScriptableSpellData spellData;
    public int positionInGroup;
    public float distanceToMove;
    private float distanceMoved;

    // For target mode - player. 
    public GameObject targetedPlayer;

    private void Start()
    {
        if (spellData.HasSprite)
        {
            GetComponent<SpriteRenderer>().sprite = spellData.ProjectileSprite;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
        }


        gameObject.GetComponent<PolygonCollider2D>().enabled = spellData.UsesCollider;
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
        // If TargetingType is PlayerStats, point towards the player
        if (spellData.TargetingType == ScriptableSpellData.TargetType.Player)
        {
            if (targetedPlayer == null)
            {
                Debug.LogWarning("Targeted player assigned as null");
            }
            else
            {
                transform.right = targetedPlayer.transform.position - transform.position; // Point towards player
            }
        }
        else if (spellData.TargetingType == ScriptableSpellData.TargetType.NotApplicable)
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
        if (spellData.TypeOfSpell == ScriptableSpellData.SpellType.Linear)
        {
            transform.position += spellData.MovementSpeed * Time.deltaTime * transform.right;
            distanceMoved += spellData.MovementSpeed * Time.deltaTime;
        }
        else if (spellData.TypeOfSpell == ScriptableSpellData.SpellType.Wall)
        {
            switch (positionInGroup)
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
        if(spellData.ScalingAfterDistance)
            UpdateScaling();
    }
    private void UpdateScaling()
    {
        float distanceForScaling = distanceToMove * spellData.ScalingStart;
        if (distanceMoved >= distanceForScaling)
        {
            float currentScale = Scaling(distanceToMove, spellData.ScalingStart, distanceMoved, spellData.SecondaryCastingArea - 1);

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

        return (scaleTargetPercentage * scalingCompletionPercentage) + 1;
    }
    private void AnimatorSetup()
    {
        // Enables the animator if AnimateSpell is set to true
        gameObject.GetComponent<Animator>().enabled = spellData.AnimateSpell;

        if (spellData.AnimateSpell)
        {
            // Sets the animation
            gameObject.GetComponent<Animator>().runtimeAnimatorController = spellData.SpellAnimatorController;
        }
    }
}
