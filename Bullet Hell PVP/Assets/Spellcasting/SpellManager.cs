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
            Debug.Log("Mana deducted, continuing casting.");
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
            Debug.Log("Casting cancelled, not enough mana available.");
        }
    }
    
    private GameObject[] InstantiateSpell(ScriptableSpellData attemptedSpellData)
    {
        GameObject[] spellObject = new GameObject[1];
        if (attemptedSpellData.ProjectileCastingArea == ScriptableSpellData.ProjectileCastArea.Single)
        {
            spellObject[0] = Instantiate(attemptedSpellData.ProjectilePrefab);
            SpellBehavior spellBehaviorScript = spellObject[0].GetComponent<SpellBehavior>();
            spellBehaviorScript.spellData = attemptedSpellData;
            spellBehaviorScript.transform.parent = transform;
        }
        else if (attemptedSpellData.ProjectileCastingArea == ScriptableSpellData.ProjectileCastArea.Line)
        {
            // Fix the length of the spellObject array
            spellObject = new GameObject[attemptedSpellData.ProjectileQuantity];

            // Loop and instantiate
            for (var i = 0; i < attemptedSpellData.ProjectileQuantity; i++)
            {
                spellObject[i] = Instantiate(attemptedSpellData.ProjectilePrefab);
                SpellBehavior spellBehaviorScript = spellObject[i].GetComponent<SpellBehavior>();
                spellBehaviorScript.spellData = attemptedSpellData;
                spellBehaviorScript.transform.parent = transform;
                // Reposition spellObject here.
            }
        }
        else
        {
            Debug.LogWarning("Projectile casting area is not yet implemented.");
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
        else
        {
            Debug.Log("Targeting type not yet implemented");
        }
    }
}
