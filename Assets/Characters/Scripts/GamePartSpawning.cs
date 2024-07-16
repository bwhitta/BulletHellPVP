using UnityEngine;
using System;
using Unity.Netcode;

public class GamePartSpawning : NetworkBehaviour
{
    [SerializeField] private GameObject cursorPrefab;
    [SerializeField] private GameObject spellbookPrefab;
    private CharacterInfo characterInfo;

    void Start()
    {
        GetReferences();
        InstantiateCursor();
        InstantiateSpellbook();
        
        void GetReferences()
        {
            characterInfo = GetComponent<CharacterStats>().characterInfo;
        }
        
        void InstantiateCursor()
        {
            // Online
            if (MultiplayerManager.IsOnline && IsServer)
            {
                // Create object locally
                GameObject cursorObject = Instantiate(cursorPrefab);
                
                SpellManager spellManagerScript = cursorObject.GetComponent<SpellManager>();
                
                // Set character info locally
                spellManagerScript.characterInfo = characterInfo;

                // Spawn on network
                cursorObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                Debug.Log($"Spawned cursor for character {characterInfo}", cursorObject);

                // Sets the characterId online
                byte characterId = (byte)Array.IndexOf(GameSettings.Used.Characters, characterInfo);
                spellManagerScript.networkCharacterId.Value = characterId;
            }
            // Local
            else if (!MultiplayerManager.IsOnline)
            {
                // Spawn object
                GameObject spellManagerObject = Instantiate(cursorPrefab);
                
                // Give it the character info
                spellManagerObject.GetComponent<SpellManager>().characterInfo = characterInfo;
            }
        }
        void InstantiateSpellbook()
        {
            // Online
            if (MultiplayerManager.IsOnline && IsServer)
            {
                // Create object locally
                GameObject spellbook = Instantiate(spellbookPrefab);

                SpellbookLogic spellbookLogic = spellbook.GetComponent<SpellbookLogic>();

                // Set character info locally
                spellbookLogic.characterInfo = characterInfo;

                // Spawn on network
                spellbook.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                Debug.Log($"Spawned spellbook for character {characterInfo}", spellbook);

                // Sets the characterId online
                byte characterId = (byte)Array.IndexOf(GameSettings.Used.Characters, characterInfo);
                spellbookLogic.networkCharacterId.Value = characterId;
            }
            // Local
            else if (!MultiplayerManager.IsOnline)
            {
                // Spawn object
                GameObject mainCanvas = GameObject.FindGameObjectWithTag(characterInfo.MainCanvasTag);
                GameObject spellbook = Instantiate(spellbookPrefab, mainCanvas.transform);

                // Give it the character info
                spellbook.GetComponent<SpellbookLogic>().characterInfo = characterInfo;
            }
        }
    }

}
