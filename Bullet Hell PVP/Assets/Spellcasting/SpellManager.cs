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
    [SerializeField] private ScriptableSpellData[] spellData;
    private string[] spellNames;

    private void Start()
    {
        spellNames = new string[spellData.Length];
        
        characterControlScript = castingPlayerObject.GetComponent<ControlCharacter>();
        
        GetSpellNames();
    }
    private void GetSpellNames()
    {
        for(var i = 0; i < spellData.Length; i++)
        {
            spellNames[i] = spellData[i].name;
        }
    }

    // Eventually add a FixedUpdate which ticks down each cooldown by Time.fixedDeltaTime

    public void CastSpell(string attemptedSpellName)
    {
        Debug.Log($"Casting spell {attemptedSpellName}");
        ScriptableSpellData attemptedSpellData = spellData[Array.IndexOf(spellNames, attemptedSpellName)];

        //Checks if the player has enough mana
        if (attemptedSpellData.ManaCost <= characterControlScript.stats.CurrentManaStat)
        {
            //Spend Mana
            characterControlScript.ModifyStat(ControlCharacter.Stat.Mana, -attemptedSpellData.ManaCost);
            // Eventually check if spell is on cooldown

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
    
    private GameObject[] InstantiateSpell(ScriptableSpellData attemptedSpellData)
    {
        GameObject[] spellObject = new GameObject[1];

        if (attemptedSpellData.CastingArea == ScriptableSpellData.CastArea.Single)
        {
            spellObject[0] = Instantiate(attemptedSpellData.ProjectilePrefab);
            SpellBehavior spellBehaviorScript = spellObject[0].GetComponent<SpellBehavior>();
            spellBehaviorScript.spellData = attemptedSpellData;

            // Reposition spell to manager
            spellBehaviorScript.transform.position = transform.position;

        }
        else if (attemptedSpellData.CastingArea == ScriptableSpellData.CastArea.Stacked)
        {
            // Fix the length of the spellObject array
            spellObject = new GameObject[attemptedSpellData.ProjectileQuantity];

            // Loop and instantiate
            for (var i = 0; i < attemptedSpellData.ProjectileQuantity; i++)
            {
                spellObject[i] = Instantiate(attemptedSpellData.ProjectilePrefab);
                SpellBehavior spellBehaviorScript = spellObject[i].GetComponent<SpellBehavior>();
                spellBehaviorScript.spellData = attemptedSpellData;

                // Reposition spell to manager
                spellBehaviorScript.transform.position = transform.position;
            }
        }
        else if (attemptedSpellData.CastingArea == ScriptableSpellData.CastArea.AdjacentCorners)
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
                spellObject[i] = Instantiate(attemptedSpellData.ProjectilePrefab);
                SpellBehavior spellBehaviorScript = spellObject[i].GetComponent<SpellBehavior>();
                
                spellBehaviorScript.spellData = attemptedSpellData;
                spellBehaviorScript.positionInGroup = i;

                spellBehaviorScript.distanceToMove = (cursorLogic.squareSide)/2;

                // Reposition spell to corner
                spellBehaviorScript.transform.SetPositionAndRotation(instantiationPoints[i], this.transform.rotation * Quaternion.Euler(0, 0, -90));
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
}
