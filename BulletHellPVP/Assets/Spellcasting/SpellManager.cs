using System;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    [SerializeField] private CharacterInfo characterInfo;

    [Space]
    //Spell data
    public ScriptableSpellData[] fullSpellData;
    private string[] _fullSpellNames;
    private string[] FullSpellNames
    {
        get
        {
            if (_fullSpellNames == null)
            {
                _fullSpellNames = new string[fullSpellData.Length];
                for (var i = 0; i < fullSpellData.Length; i++)
                {
                    _fullSpellNames[i] = fullSpellData[i].name;
                }
            }
            return _fullSpellNames;
        }
    }
    
    [Space]
    // Equipped Spells
    public string[] equippedSpellNames;

    private ScriptableSpellData[] _equippedSpellData;
    [HideInInspector] public ScriptableSpellData[] EquippedSpellData
    {
        get
        {
            if (_equippedSpellData == null)
            {
                _equippedSpellData = new ScriptableSpellData[equippedSpellNames.Length];
                for (var i = 0; i < equippedSpellNames.Length; i++)
                {
                    _equippedSpellData[i] = GetSpellData(equippedSpellNames[i]);
                }
            }
            return _equippedSpellData;
        }
    }
    // Spellbook
    [SerializeField] private GameObject spellbookObject;
    // Mask layers
    [SerializeField] private string spellMaskLayer;
    
    // Casting and instantiating spells
    public void CastSpell(string attemptedSpellName)
    {
        Debug.Log($"Casting spell {attemptedSpellName}");
        ScriptableSpellData attemptedSpellData = GetSpellData(attemptedSpellName);
        
        if (attemptedSpellData == null)
        {
            Debug.LogWarning("Casting cancelled - spell data null.");
            return;
        }

        int cooldownIndex = Array.IndexOf(FullSpellNames, attemptedSpellName);

        //Check if spell is on cooldown
        if (characterInfo.SpellbookLogicScript.spellCooldowns[cooldownIndex] > 0)
        {
            Debug.Log("Spell on cooldown.");
            return;
        }

        //Checks if the character has enough mana
        if (attemptedSpellData.ManaCost <= characterInfo.CharacterStatsScript.CurrentManaStat)
        {
            //Spend Mana
            characterInfo.CharacterStatsScript.CurrentManaStat -= attemptedSpellData.ManaCost;

            characterInfo.SpellbookLogicScript.spellCooldowns[cooldownIndex] = attemptedSpellData.SpellCooldown;

            // Instantiate the spell
            GameObject[] spellObjects = InstantiateSpell(attemptedSpellData);
            SpellBehavior[] spellBehaviors = new SpellBehavior[spellObjects.Length];

            for (var i = 0; i < spellObjects.Length; i++)
            {
                spellBehaviors[i] = spellObjects[i].GetComponent<SpellBehavior>();
            }
            
            SpellTargets(attemptedSpellData, spellBehaviors);
        }
        else
        {
            return;
        }
    }
    private GameObject[] InstantiateSpell(ScriptableSpellData spellData)
    {
        GameObject[] spellObject = new GameObject[1];


        if (spellData.CastingArea == ScriptableSpellData.CastArea.Single)
        {
            spellObject[0] = Instantiate(spellData.ProjectilePrefab);

            // Sets spell behavior
            SpellBehavior spellBehaviorScript = spellObject[0].GetComponent<SpellBehavior>();
            spellBehaviorScript.spellData = spellData;

            // Reposition spell to manager
            spellBehaviorScript.transform.position = transform.position;

            // Add mask tag
            spellObject[0].GetComponent<Renderer>().sortingLayerName = spellMaskLayer;

            // Set up animated children (if enabled for the spell)
            AnimatedChildrenSetup(spellObject[0], spellData);
        }
        else if (spellData.CastingArea == ScriptableSpellData.CastArea.Stacked)
        {
            // Fix the length of the spellObject array
            spellObject = new GameObject[spellData.ProjectileQuantity];

            // Loop and instantiate
            for (var i = 0; i < spellData.ProjectileQuantity; i++)
            {
                spellObject[i] = Instantiate(spellData.ProjectilePrefab);
                SpellBehavior spellBehaviorScript = spellObject[i].GetComponent<SpellBehavior>();
                spellBehaviorScript.spellData = spellData;

                // Reposition spell to manager
                spellBehaviorScript.transform.position = transform.position;

                // Add mask tag
                spellObject[i].GetComponent<Renderer>().sortingLayerName = spellMaskLayer;

                // Set up animated children (if enabled for the spell)
                AnimatedChildrenSetup(spellObject[i], spellData);
            }
        }
        else if (spellData.CastingArea == ScriptableSpellData.CastArea.AdjacentCorners)
        {
            CursorLogic cursorLogic = this.GetComponent<CursorLogic>();
            int cursorWall = cursorLogic.GetCurrentWall();
            Vector2[] corners = cursorLogic.GetCurrentSquareCorners();
            
            // Points to instantiate at
            Vector2[] instantiationPoints = new Vector2[]
            {
                corners[cursorWall],
                corners[(cursorWall + 1) % 4]
            };
            
            // Fix the length of the spellObject array
            spellObject = new GameObject[2];
            // Loop and instantiate
            for (var i = 0; i < spellObject.Length; i++)
            {
                //Instantiate the spell
                spellObject[i] = Instantiate(spellData.ProjectilePrefab);

                // The script on the instantiated spell
                SpellBehavior spellBehaviorScript = spellObject[i].GetComponent<SpellBehavior>(); 
                
                // Send data to the spell's script
                spellBehaviorScript.spellData = spellData;
                spellBehaviorScript.positionInGroup = i;
                spellBehaviorScript.distanceToMove = (cursorLogic.squareSide)/2;

                // Reposition spell to corner, rotate to match manager.
                spellBehaviorScript.transform.SetPositionAndRotation(instantiationPoints[i], this.transform.rotation * Quaternion.Euler(0, 0, -90));

                // Add mask tag
                spellObject[i].GetComponent<Renderer>().sortingLayerName = spellMaskLayer;

                // Set up animated children (if enabled for the spell)
                AnimatedChildrenSetup(spellObject[i], spellData);
            }
        }
        else
        {
            Debug.LogWarning("This casting area is not yet implemented.");
        }
        return spellObject;
    }

    // Spell configuration - run from CastSpell and InstantiateSpell
    private void SpellTargets(ScriptableSpellData attemptedSpellData, SpellBehavior[] spellBehaviors)
    {
        if (attemptedSpellData.TargetingType == ScriptableSpellData.TargetType.Character)
        {
            for (int i = 0; i < spellBehaviors.Length; i++)
            {
                spellBehaviors[i].targetedCharacter = characterInfo.opponentCharacterInfo.CharacterObject;
            }
        }
        else if (attemptedSpellData.TargetingType == ScriptableSpellData.TargetType.NotApplicable)
        {
            // N/A, nothing should happen
            return;
        }
        else
        {
            Debug.Log("Targeting type not yet implemented");
        }
    }
    private void AnimatedChildrenSetup(GameObject spellPrefabObject, ScriptableSpellData spellData)
    {
        if(spellData.AnimateSpell == false)
        {
            return;
        }
        // SpellBehavior spellPrefabScript = spellPrefabObject.GetComponent<SpellBehavior>();
        for(var i = 0; i < spellData.MultipartAnimationPrefabs.Length; i++)
        {
            GameObject currentAnimationPrefab = Instantiate(spellData.MultipartAnimationPrefabs[i]);
            currentAnimationPrefab.transform.parent = spellPrefabObject.transform;

            currentAnimationPrefab.transform.SetPositionAndRotation(spellPrefabObject.transform.position, spellPrefabObject.transform.rotation);

            // Animator does not work with changed name, so this line resets the name.
            currentAnimationPrefab.name = spellData.MultipartAnimationPrefabs[i].name;
            currentAnimationPrefab.GetComponent<Renderer>().sortingLayerName = spellMaskLayer;
        }
    }
    private ScriptableSpellData GetSpellData(string spellName)
    {
        //Gets the index of the spell
        int spellIndex = Array.IndexOf(FullSpellNames, spellName);

        if (spellIndex == -1)
        {
            Debug.LogWarning($"The spell name {spellName} is not in fullSpellNames.");
            return null;
        }

        //Returns the spell at that index
        return fullSpellData[spellIndex];
    }
}
