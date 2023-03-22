using System;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.U2D.Path;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [SerializeField] private CharacterInfo characterInfo;
    [SerializeField] private string spellMaskLayer;

    // References
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
    
    // Casting and instantiating spells
    public void AttemptSpell(int equippedIndex)
    {
        SpellData attemptedSpellData = characterInfo.EquippedSpells[equippedIndex];
        // Check mana
        if (attemptedSpellData.ManaCost >= characterInfo.CharacterStats.CurrentManaStat)
        {
            Debug.Log("Not enough mana.");
            return;
        }
        else
        {
            characterInfo.CharacterStats.CurrentManaStat -= attemptedSpellData.ManaCost;
        }
        // Check cooldown
        if (characterInfo.SpellbookLogicScript.spellCooldowns[equippedIndex] > 0)
        {
            Debug.Log("Spell on cooldown.");
            return;
        }
        else
        {
            characterInfo.SpellbookLogicScript.spellCooldowns[equippedIndex] = attemptedSpellData.SpellCooldown;
        }
        

        ResolveSpell(attemptedSpellData);

        // Local Method
        void ResolveSpell(SpellData spellData)
        {
            SpellBehavior[] spellBehaviors = CreateSpell(spellData);

            foreach (SpellBehavior spell in spellBehaviors)
            {
                SpellDisplaySetup(spell, spellData);
                SpellTargets(spellData, spell);
            }
        }
    }

    
    private SpellBehavior[] CreateSpell(SpellData spellData)
    {
        SpellBehavior[] spellBehaviors = InstantiateSpell(spellData);
        
        for (var i = 0; i < spellBehaviors.Length; i++)
        {
            SpellBehavior spell = spellBehaviors[i];

            spell.indexWithinSpell = i;
            ConfigureBehavior(spell, spellData);
        }
        return spellBehaviors;

        // Local Methods
        void ConfigureBehavior(SpellBehavior behavior, SpellData spellData)
        {
            switch (spellData.SpawningArea)
            {
                case SpellData.SpawningAreas.Points:
                    behavior.spellData = spellData;
                    behavior.transform.position = transform.position;
                    break;
                case SpellData.SpawningAreas.AdjacentCorners:
                    behavior.spellData = spellData;
                    behavior.distanceToMove = (CursorLogic.squareSide) / 2;
                    
                    Debug.Log($"Square side: {CursorLogic.squareSide}.");
                    behavior.transform.position = CalculateAdjacentCorners()[behavior.indexWithinSpell];
                    behavior.transform.rotation = this.transform.rotation * Quaternion.Euler(0, 0, -90);
                    break;
                default:
                    Debug.LogWarning($"Casting Area {spellData.SpawningArea} does not exist.");
                    break;
            }

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
        SpellBehavior[] InstantiateSpell(SpellData spellData)
        {
            SpellBehavior[] spellBehaviors;
            spellBehaviors = new SpellBehavior[spellData.InstantiationQuantity];
            for (var i = 0; i < spellData.InstantiationQuantity; i++)
            {
                spellBehaviors[i] = Instantiate(spellData.Prefab).GetComponent<SpellBehavior>();
            }
            return spellBehaviors;
        }
    }


    private void SpellDisplaySetup(SpellBehavior spellBehavior, SpellData spellData)
    {
        spellBehavior.GetComponent<Renderer>().sortingLayerName = spellMaskLayer;
        if (spellData.AnimatedSpell)
        {
            AnimatedChildrenSetup(spellData, spellBehavior);
        }
    }

    private void SpellTargets(SpellData spellData, SpellBehavior spellBehavior)
    {
        if (spellData.TargetingType == SpellData.TargetTypes.Character)
        {
            spellBehavior.targetedCharacter = characterInfo.OpponentCharacterInfo.CharacterObject;
        }
        else if (spellData.TargetingType == SpellData.TargetTypes.NotApplicable)
        {
            // N/A, nothing should happen
            return;
        }
        else
        {
            Debug.Log("Targeting type not yet implemented");
        }
    }
    private void AnimatedChildrenSetup(SpellData spellData, SpellBehavior spellBehavior)
    {
        for (var i = 0; i < spellData.MultipartAnimationPrefabs.Length; i++)
        {
            GameObject currentAnimationPrefab = Instantiate(spellData.MultipartAnimationPrefabs[i], spellBehavior.transform);

            currentAnimationPrefab.transform.SetPositionAndRotation(spellBehavior.transform.position, spellBehavior.transform.rotation);
            currentAnimationPrefab.transform.localScale = Vector3.one;

            // Animator does not work with changed name, so this line resets the name.
            currentAnimationPrefab.name = spellData.MultipartAnimationPrefabs[i].name;

            // Set the mask layer
            currentAnimationPrefab.GetComponent<Renderer>().sortingLayerName = spellMaskLayer;
        }
    }
}
