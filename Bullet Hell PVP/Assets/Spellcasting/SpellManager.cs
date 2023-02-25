using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
        [Header("Object References")]
    [SerializeField] private GameObject castingPlayerObject;
    private ControlCharacter characterControlScript;
    
        [Header("Spells")]
    public ScriptableSpellData[] fullSpellData;
    private string[] fullSpellNames;
    [Space] // Equipped spells
    public string[] equippedSpellNames;
    [HideInInspector] public ScriptableSpellData[] equippedSpellData;
    [Space] // Spellbook
    [SerializeField] private GameObject spellbookObject;
    private SpellbookLogic spellbookLogic;
    // Cooldown
    private float[] spellCooldowns;

    private void Start()
    {
        GetSpellNames();

        characterControlScript = castingPlayerObject.GetComponent<ControlCharacter>();

        //Enable spell controls from the SpellbookLogic script
        SetEquippedSpellData();
        spellbookLogic = spellbookObject.GetComponent<SpellbookLogic>();
        spellbookLogic.EnableSpellControls();
    }
    private void Update()
    {
        UpdateCooldown();   
    }

    private void GetSpellNames()
    {
        fullSpellNames = new string[fullSpellData.Length];
        for (var i = 0; i < fullSpellData.Length; i++)
        {
            fullSpellNames[i] = fullSpellData[i].name;
        }
    }
    private void SetEquippedSpellData()
    {
        equippedSpellData = new ScriptableSpellData[equippedSpellNames.Length];
        for(var i = 0; i < equippedSpellNames.Length; i++)
        {
            equippedSpellData[i] = GetSpellData(equippedSpellNames[i]);
        }
    }
    private void UpdateCooldown()
    {
        // Set up cooldowns if data is invalid
        if (spellCooldowns == null ||  spellCooldowns.Length < equippedSpellNames.Length)
        {
            spellCooldowns = new float[equippedSpellNames.Length];
        }

        // Loop through cooldowns and tick down by time.deltatime
        for(int i = 0; i < spellCooldowns.Length; i++)
        {
            if (spellCooldowns[i] > 0)
            {

                spellCooldowns[i] -= Time.deltaTime;
            }
            if(spellCooldowns[i] < 0)
            {
                spellCooldowns[i] = 0;
            }
            //Updates the cooldown UI for i with the current percent
            spellbookLogic.UpdateCooldownUI(i, spellCooldowns[i] / equippedSpellData[i].SpellCooldown);
        }
    }

    public void CastSpell(string attemptedSpellName)
    {
        // Debug.Log($"Casting spell {attemptedSpellName}");
        ScriptableSpellData attemptedSpellData = GetSpellData(attemptedSpellName);
        
        if (attemptedSpellData == null)
        {
            Debug.LogWarning("Casting cancelled - spell data null.");
            return;
        }

        int cooldownIndex = Array.IndexOf(fullSpellNames, attemptedSpellName);

        //Check if spell is on cooldown
        if (spellCooldowns[cooldownIndex] > 0)
        {
            Debug.Log("Spell on cooldown.");
            return;
        }

        //Checks if the player has enough mana
        if (attemptedSpellData.ManaCost <= characterControlScript.stats.CurrentManaStat)
        {
            //Spend Mana
            characterControlScript.ModifyStat(ControlCharacter.Stat.Mana, -attemptedSpellData.ManaCost);

            spellCooldowns[cooldownIndex] = attemptedSpellData.SpellCooldown;

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
    private void SpellTargets(ScriptableSpellData attemptedSpellData, SpellBehavior[] spellBehaviors)
    {
        if (attemptedSpellData.TargetingType == ScriptableSpellData.TargetType.Player)
        {
            for (int i = 0; i < spellBehaviors.Length; i++)
            {
                spellBehaviors[i].targetedPlayer = castingPlayerObject;
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
        }
    }
    private ScriptableSpellData GetSpellData(string spellName)
    {
        //Gets the index of the spell
        int spellIndex = Array.IndexOf(fullSpellNames, spellName);

        if (spellIndex == -1)
        {
            Debug.LogWarning($"The spell name {spellName} is not in fullSpellNames.");
            return null;
        }

        //Returns the spell at that index
        return fullSpellData[spellIndex];
    }
}
