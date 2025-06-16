using System;
using UnityEngine;

public class CharacterCollisions : MonoBehaviour
{
    // Fields
    private CharacterStats characterStats;
    private CharacterManager characterManager;
    private float remainingInvincibilityTime = 0;

    // Properties
    private bool CharacterInvincible
    {
        get
        {
            return remainingInvincibilityTime > 0;
        }
    }

    // Methods
    private void Start()
    {
        // Get references
        characterStats = GetComponent<CharacterStats>();
        characterManager = GetComponent<CharacterManager>();

        // Subscribe to health changed event
        //characterStats.HealthChanged += 
    }

    private void FixedUpdate()
    {
        if (remainingInvincibilityTime > 0)
        {
            InvincibilityTick();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameObject.activeSelf == false)
        {
            Debug.Log($"gameObject is not active"); // temp log
            return;
        }
        if (collision.TryGetComponent<Spell>(out var spell))
        {
            if (!CharacterInvincible && spell.Module.DealsDamage && spell.TargetId == characterManager.CharacterIndex)
            {
                characterStats.CurrentHealth -= spell.Module.Damage;
                SetInvincibility();
            }
        }


    }
    private void SetInvincibility()
    {
        remainingInvincibilityTime = GameSettings.Used.InvincibilityTime;
        SetChildAlpha(GameSettings.Used.InvincibilityAlphaMod);
    }
    private void InvincibilityTick()
    {
        remainingInvincibilityTime -= Time.fixedDeltaTime;

        // Check if completed
        if (remainingInvincibilityTime <= 0)
        {
            remainingInvincibilityTime = 0;
            SetChildAlpha(1);
        }
    }
    private void SetChildAlpha(float alpha)
    {
        foreach (Transform child in transform)
        {
            child.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        }
    }
}
