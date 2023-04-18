using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [SerializeField] private CharacterInfo characterInfo;
    [SerializeField] private GameObject spellbookObject;

    private CursorLogic _cursorLogic;
    private CursorLogic CursorLogic
    {
        get
        {
            if (_cursorLogic == null)
            {
                _cursorLogic = GetComponent<CursorLogic>();
            }
            return _cursorLogic;
        }
    }
    

    public void AttemptSpell(int spellIndex)
    {
        SpellData[] currentBook = characterInfo.EquippedSpellBooks[characterInfo.CurrentBook];
        // Check if the slot is valid
        if (spellIndex < 0 || currentBook.Length <= spellIndex || currentBook[spellIndex] == null)
        {
            Debug.Log($"No spell in slot {spellIndex}");
            return;
        }

        // Gets the spell in the slot
        SpellData spellData = characterInfo.EquippedSpellBooks[characterInfo.CurrentBook][spellIndex];
        
        // Check cooldown and mana
        if (CooldownAndManaAvailable() == false)
            return;

        foreach(SpellData.Module module in spellData.UsedModules)
        {
            // Instantiate the spell
            SpellModuleBehavior[] moduleBehaviors = InstantiateModule(module);
            // Configure the spell
            for (int i = 0; i < moduleBehaviors.Length; i++)
            {
                ConfigureModule(moduleBehaviors[i], module, i);
            }
        }

        // Local Method:
        bool CooldownAndManaAvailable()
        {
            if (characterInfo.SpellbookLogicScript.spellCooldowns[spellIndex] > 0)
            {
                Debug.Log("Spell on cooldown.");
                return false;
            }
            else if (spellData.ManaCost >= characterInfo.CharacterStats.CurrentManaStat)
            {
                Debug.Log("Not enough mana.");
                return false;
            }
            else
            {
                characterInfo.SpellbookLogicScript.spellCooldowns[spellIndex] = spellData.SpellCooldown;
                characterInfo.CharacterStats.CurrentManaStat -= spellData.ManaCost;
                return true;
            }
        }
    }
    private SpellModuleBehavior[] InstantiateModule(SpellData.Module module)
    {
        SpellModuleBehavior[] spellBehaviors = new SpellModuleBehavior[module.InstantiationQuantity];
        for (var i = 0; i < module.InstantiationQuantity; i++)
        {
            spellBehaviors[i] = Instantiate(module.Prefab).GetComponent<SpellModuleBehavior>();
        }

        return spellBehaviors;
    }
    private void ConfigureModule(SpellModuleBehavior moduleBehavior, SpellData.Module module, int indexWithinSpell)
    {
        // Give the spell its ID
        moduleBehavior.spellBehaviorID = indexWithinSpell;

        switch (module.ModuleType)
        {
            case SpellData.ModuleTypes.Projectile:
                SetProjectileTransform();
                break;
            case SpellData.ModuleTypes.PlayerAttached:
                AttachToPlayer();
                break;
        }


        GiveSpellTargets();
        moduleBehavior.module = module;
        
        #region LocalMethods
        void SetProjectileTransform()
        {
            switch (module.ProjectileSpawningArea)
            {
                case SpellData.SpawningAreas.Point:
                    moduleBehavior.transform.position = transform.position;
                    break;
                case SpellData.SpawningAreas.AdjacentCorners:
                    moduleBehavior.distanceToMove = (CursorLogic.squareSide) / 2;
                    moduleBehavior.transform.position = CalculateAdjacentCorners()[moduleBehavior.spellBehaviorID];
                    moduleBehavior.transform.rotation = this.transform.rotation * Quaternion.Euler(0, 0, -90);
                    break;
                default:
                    Debug.LogWarning($"Casting Area {module.ProjectileSpawningArea} does not exist.");
                    break;
            }

            moduleBehavior.spellMaskLayer = characterInfo.OpponentCharacterInfo.CharacterAndSortingTag;

            // Local Method
            Vector2[] CalculateAdjacentCorners()
            {
                int cursorWall = CursorLogic.GetCurrentWall();
                Vector2[] corners = CursorLogic.GetCurrentSquareCorners();
                return new Vector2[]
                {
                corners[cursorWall],
                corners[(cursorWall + 1) % 4]
                };
            }
        }
        void AttachToPlayer()
        {
            moduleBehavior.transform.parent = characterInfo.CharacterObject.transform;
            moduleBehavior.transform.localPosition = Vector3.zero;
            moduleBehavior.spellMaskLayer = characterInfo.CharacterAndSortingTag;
        }
        void GiveSpellTargets()
        {
            switch (module.TargetingType)
            {
                case SpellData.TargetTypes.Character:
                    moduleBehavior.targetedCharacter = characterInfo.OpponentCharacterInfo.CharacterObject;
                    break;
                case SpellData.TargetTypes.Center:
                    // Not yet implemented
                    break;
                case SpellData.TargetTypes.Opposing:
                    // Not yet implemented
                    break;
                case SpellData.TargetTypes.InvertedOpposing:
                    // Not yet implemented
                    break;
            }
        }
        #endregion
    }
}
