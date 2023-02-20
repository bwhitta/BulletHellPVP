using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static SpellManager;

public class SpellBehavior : MonoBehaviour
{
    public ScriptableSpellData spellData;

    // For target mode - player. 
    public GameObject targetedPlayer;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = spellData.ProjectileSprite;
        PointTowardsTarget();

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
        // If TargetingType is Player, point towards the player
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
        else
        {
            Debug.LogWarning("Targeting type is not yet implemented.");
        }
    }

    private void MoveSpell()
    {
        if(spellData.MovementType == ScriptableSpellData.ProjectileMovementType.Linear)
        {
            transform.position += spellData.MovementSpeed * Time.deltaTime * transform.right;
        }
    }
}
