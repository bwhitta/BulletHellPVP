using UnityEngine;
using static BarLogic;

public class CharacterStats : MonoBehaviour
{

    public CharacterInfo characterInfo;

    private float remainingInvincibilityTime = 0;

    // Monobehavior methods
    private void Update()
    {
        InvincibilityTick();
    }
    private void OnEnable()
    {
        CharacterEnabled(true);
    }
    private void OnDisable()
    {
        CharacterEnabled(false);
    }
    
    // Actions upon enabling or disabling character
    private void CharacterEnabled(bool enable)
    {
        if (enable)
        {
            // Get character info
            characterInfo = CharacterInfoManager.JoinAvailableLocation();
            if (characterInfo == null)
            {
                Debug.Log("Character destroyed - no available slot found");
                Destroy(gameObject);
                return;
            }
            // Debug.Log($"Set character info to {characterInfo.name}");

            transform.position = characterInfo.CharacterStartLocation;
        }
        else
        {
            if (characterInfo == null || characterInfo.SpellbookLogicScript == null)
            {
                Debug.Log("Skipping disable, some info was found null");
                return;
            }
        }

        // Check tag
        if (gameObject.CompareTag("Untagged"))
        {
            gameObject.tag = characterInfo.CharacterAndSortingTag;
        }

        // Enable other objects
        characterInfo.HealthBar.BarEnabled(enable);
        characterInfo.ManaBar.BarEnabled(enable);
        characterInfo.SpellbookLogicScript.SpellbookToggle(enable);
    }

    // Health
    private float _currentHealthStat;
    public float CurrentHealthStat
    {
        get
        {
            StatConfigCheck();
            return _currentHealthStat;
        }
        set
        {
            StatConfigCheck();
            if (value > characterInfo.DefaultStats.MaxHealthStat)
                _currentHealthStat = characterInfo.DefaultStats.MaxHealthStat;

            else
                _currentHealthStat = value;
            characterInfo.HealthBar.UpdateStatDisplays(UpdatableStats.Remaining);

            if (value < 0)
            {
                _currentHealthStat = 0;
                Debug.Log("dead");
                Destroy(gameObject);
            }
        }
    }

    // Mana
    private float _currentManaStat;
    public float CurrentManaStat
    {
        get
        {
            StatConfigCheck();
            return _currentManaStat;
        }
        set
        {
            StatConfigCheck();
            if (value > characterInfo.DefaultStats.MaxManaStat)
                _currentManaStat = characterInfo.DefaultStats.MaxManaStat;
            else
                _currentManaStat = value;
            characterInfo.ManaBar.UpdateStatDisplays(UpdatableStats.Remaining);
        }
    }

    // Check if the stats are configured
    private bool statsConfigured;
    private void StatConfigCheck()
    {
        if (!statsConfigured)
        {
            _currentHealthStat = characterInfo.DefaultStats.MaxHealthStat;
            _currentManaStat = characterInfo.DefaultStats.MaxManaStat;
        }
        statsConfigured = true;
    }

    // Collision
    private void OnTriggerEnter2D(Collider2D collision)
    {
        CheckCollision(collision);
    }
    private void CheckCollision(Collider2D collision)
    {
        Debug.Log($"{gameObject.name} is colliding with {collision.gameObject.name}");
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (collision.GetComponent<SpellModuleBehavior>() != null)
        {
            SpellModuleBehavior collisionSpellBehavior = collision.GetComponent<SpellModuleBehavior>();
            
            if(remainingInvincibilityTime <= 0)
            {
                if (collisionSpellBehavior.module.AbilityDealsDamage)
                {
                    remainingInvincibilityTime = characterInfo.DefaultStats.InvincibilityTime;
                    gameObject.GetComponent<CharacterStats>().CurrentHealthStat -= collisionSpellBehavior.module.Damage;
                    Debug.Log($"{collisionSpellBehavior.module.Damage} health lost ");
                }
                else
                {
                    Debug.Log($"AbilityDealsDamage is false for {collisionSpellBehavior}");
                }
            }
        }
    }
    
    // Invincibility after damage
    private void InvincibilityTick()
    {
        if (remainingInvincibilityTime > 0)
        {
            remainingInvincibilityTime -= Time.deltaTime;

            SetChildAlpha(characterInfo.DefaultStats.InvincibilityAlphaMod);
        }
        if (remainingInvincibilityTime < 0)
        {
            remainingInvincibilityTime = 0;
            SetChildAlpha(1);
        }

        CurrentManaStat += characterInfo.DefaultStats.BaseManaRegen * Time.deltaTime;
    }
    private float currentAlpha;
    private void SetChildAlpha(float alpha)
    {
        if(Mathf.Approximately(currentAlpha, alpha))
        {
            return;
        }
        currentAlpha = alpha;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            gameObject.transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, alpha);
        }
    }
}
