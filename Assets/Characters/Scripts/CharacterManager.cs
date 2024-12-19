using UnityEngine;
using System;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    public CharacterParts Parts = new();
    
    [SerializeField] private GameObject cursorPrefab;
    [SerializeField] private GameObject spellbookPrefab;
    [SerializeField] private GameObject statBarPrefab;

    [HideInInspector] public byte CharacterIndex;

    void Start()
    {
        if (MultiplayerManager.IsOnline)
        {
            if ((IsHost && IsOwner) || (!IsHost && !IsOwner))
            {
                CharacterIndex = 0;
            }
            else
            {
                CharacterIndex = 1;
            }
        }

        Parts.OwnedCharacterInfo = GameSettings.Used.Characters[CharacterIndex];
        if (CharacterIndex == 0) OpponentCharacterInfo = GameSettings.Used.Characters[1];
        else OpponentCharacterInfo = GameSettings.Used.Characters[0];

        name = OwnedCharacterInfo.name;

        // Alternatively, I could just give these a reference to this class and make that the way they get the characterInfo.
        CharacterControls characterControls = GetComponent<CharacterControls>();
        characterControls.characterInfo = OwnedCharacterInfo; //REMOVE CHARACTERINFO FROM CHARACTERCONTROLS
        characterControls.Startup();
        
        InstantiateCursor();
        InstantiateSpellbook();
        HealthBar = InstantiateStatBar(GameSettings.Visuals.HealthBarColors);
        ManaBar = InstantiateStatBar(GameSettings.Visuals.ManaBarColors);

        //Local Methods
        void InstantiateCursor()
        {
            if (!MultiplayerManager.IsOnline)
            {
                CursorObject = Instantiate(cursorPrefab);
                CursorObject.GetComponent<SpellManager>().characterPartsManager = this;
            }
            if (MultiplayerManager.IsOnline && IsServer)
            {
                // Create object locally
                CursorObject = Instantiate(cursorPrefab);
                SpellManager spellManagerScript = CursorObject.GetComponent<SpellManager>();
                spellManagerScript.characterPartsManager = this;

                // Spawn on network
                CursorObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                Debug.Log($"Spawned cursor for character {OwnedCharacterInfo}", CursorObject);

                // Sets the characterId online
                byte characterIndex = (byte)Array.IndexOf(GameSettings.Used.Characters, OwnedCharacterInfo);
                spellManagerScript.networkCharacterIndex.Value = characterIndex;
            }
        }
        void InstantiateSpellbook()
        {
            if (!MultiplayerManager.IsOnline)
            {
                SpellbookObject = Instantiate(spellbookPrefab);
                SpellbookObject.GetComponent<SpellbookLogic>().characterInfo = OwnedCharacterInfo;
            }
            else if (MultiplayerManager.IsOnline && IsServer)
            {
                // Create object locally
                SpellbookObject = Instantiate(spellbookPrefab);
                SpellbookLogic spellbookLogic = SpellbookObject.GetComponent<SpellbookLogic>();
                spellbookLogic.characterInfo = OwnedCharacterInfo;

                // Spawn on network
                SpellbookObject.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
                Debug.Log($"Spawned spellbook for character {OwnedCharacterInfo}", SpellbookObject);

                // Sets the characterId online
                byte characterIndex = (byte)Array.IndexOf(GameSettings.Used.Characters, OwnedCharacterInfo);
                spellbookLogic.networkCharacterId.Value = characterIndex;
            }
        }
        BarLogic InstantiateStatBar(VisualSettings.BarColors barColors)
        {
            if (MultiplayerManager.IsOnline)
            {
                Debug.LogWarning($"I'm not sure if stat bars will work online yet");
            }
            GameObject statBar = Instantiate(statBarPrefab);
            BarLogic barLogic = statBar.GetComponent<BarLogic>();
            barLogic.BarColors = barColors;
            return barLogic;
        }
    }
}
