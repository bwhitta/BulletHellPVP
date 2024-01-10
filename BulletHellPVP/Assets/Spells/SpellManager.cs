using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class SpellManager : NetworkBehaviour
{
    [HideInInspector] public CharacterInfo characterInfo;
    public readonly NetworkVariable<Byte> networkCharacterId = new();

    private void Start()
    {
        // Set the tag on local or server instances
        if (!MultiplayerManager.IsOnline || IsServer)
        {
            tag = characterInfo.CharacterObject.tag;
        }
        // Set the tag for non-host client instances
        else
        {
            Debug.Log($"START: CharacterId is {networkCharacterId.Value}");
            
            // Set characterInfo and tag
            characterInfo = GameSettings.Used.Characters[networkCharacterId.Value];
            tag = characterInfo.CharacterObject.tag;

            // If the character ID changes, update the characterInfo and tag.
            networkCharacterId.OnValueChanged += NetworkCharacterIdChanged;
        }
        GetComponent<CursorLogic>().CharacterInfoSet();

        void NetworkCharacterIdChanged(byte prev, byte changedTo)
        {
            Debug.Log($"ID CHANGED: CharacterId is {networkCharacterId.Value}");
            characterInfo = GameSettings.Used.Characters[changedTo];
            tag = characterInfo.CharacterObject.tag;
            
            // Update cursorLogic to make sure it uses the new info
            CursorLogic cursorLogic = GetComponent<CursorLogic>();
            cursorLogic.CharacterInfoSet();
            cursorLogic.UpdateCursor();
        }
    }

    public static SpellData GetSpellData(byte setIndex, byte spellIndex)
    {
        SpellSetInfo set = GameSettings.Used.SpellSets[setIndex];
        if(spellIndex >= set.spellsInSet.Length)
        {
            Debug.LogWarning($"Set {set.name} does not have a spell at index {spellIndex}.");
        }
        return set.spellsInSet[spellIndex];
    }
    public static SpellData GetSpellData(CharacterInfo.Spellbook currentBook, byte slot)
    {
        return GetSpellData(currentBook.SetIndexes[slot], currentBook.SpellIndexes[slot]);
    }

    public void AttemptSpell(byte slot)
    {
        // Check slot validity
        if (GetSpellData(characterInfo.CurrentBook, slot) == null)
        {
            Debug.Log($"No spell in slot {slot}");
            return;
        }

        // Gets the spell in the slot
        SpellData spellData = GetSpellData(characterInfo.CurrentBook, slot);

        // Check cooldown and mana
        if (CooldownAndManaAvailable() == false)
            return;

        Debug.Log($"Starting instantation of spell in {slot}");
        for (byte i = 0; i < spellData.UsedModules.Length; i++)
        {
            SpellData.Module module = spellData.UsedModules[i];

            SpellModuleBehavior[] moduleBehaviors = InstantiateModule(module);

            for (byte j = 0; j < moduleBehaviors.Length; j++)
            {
                Debug.Log($"Sending the data to the server's behaviors (will later be sent to clients)");
                SpellModuleBehavior behavior = moduleBehaviors[j]; 
                behavior.setIndex = characterInfo.CurrentBook.SetIndexes[slot];
                behavior.spellIndex = characterInfo.CurrentBook.SpellIndexes[slot];
                behavior.moduleIndex = i;
                behavior.behaviorID = j;
                behavior.ownerID = (byte)Array.IndexOf(GameSettings.Used.Characters, characterInfo);

                if (IsServer)
                {
                    NetworkObject moduleObject = behavior.gameObject.GetComponent<NetworkObject>();
                    moduleObject.Spawn(true);
                    Debug.Log($"Spawned behavior {behavior}");
                }
                else
                {
                    Debug.LogWarning($"This client isn't a server.");
                }
            }
        }

        // Local Methods
        bool CooldownAndManaAvailable()
        {
            if (characterInfo.SpellbookLogicScript.spellCooldowns[slot] > 0)
            {
                Debug.Log("Spell on cooldown.");
                return false;
            }
            else if (spellData.ManaCost > characterInfo.CharacterStats.CurrentManaStat)
            {
                Debug.Log("Not enough mana.");
                return false;
            }
            else
            {
                characterInfo.SpellbookLogicScript.spellCooldowns[slot] = spellData.SpellCooldown;
                characterInfo.CharacterStats.CurrentManaStat -= spellData.ManaCost;
                return true;
            }
        }
    }

    [ServerRpc]
    public void AttemptSpellServerRpc(byte slotIndex)
    {
        Debug.Log($"ServerRpc recieved, attempting spell server-side");
        AttemptSpell(slotIndex);
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
}   
