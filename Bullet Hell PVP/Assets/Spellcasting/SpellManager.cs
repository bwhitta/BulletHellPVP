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
    [SerializeField] private ScriptableSpellData[] fullSpellData;
    private string[] spellNames;

    private void Start()
    {
        spellNames = new string[fullSpellData.Length];
        
        characterControlScript = castingPlayerObject.GetComponent<ControlCharacter>();
        
        GetSpellNames();
    }
    private void GetSpellNames()
    {
        for(var i = 0; i < fullSpellData.Length; i++)
        {
            spellNames[i] = fullSpellData[i].name;
        }
    }

    // Eventually add a FixedUpdate which ticks down each cooldown by Time.fixedDeltaTime

    public void CastSpell(string attemptedSpellName)
    {
        Debug.Log($"Casting spell {attemptedSpellName}");
        ScriptableSpellData attemptedSpellData = fullSpellData[Array.IndexOf(spellNames, attemptedSpellName)];

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
    
    private GameObject[] InstantiateSpell(ScriptableSpellData spellData)
    {
        GameObject[] spellObject = new GameObject[1];

        if (spellData.CastingArea == ScriptableSpellData.CastArea.Single)
        {
            spellObject[0] = Instantiate(spellData.ProjectilePrefab);
            SpellBehavior spellBehaviorScript = spellObject[0].GetComponent<SpellBehavior>();
            spellBehaviorScript.spellData = spellData;

            // Reposition spell to manager
            spellBehaviorScript.transform.position = transform.position;

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

                // Reposition spell to corner
                spellBehaviorScript.transform.SetPositionAndRotation(instantiationPoints[i], this.transform.rotation * Quaternion.Euler(0, 0, -90));

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
}
