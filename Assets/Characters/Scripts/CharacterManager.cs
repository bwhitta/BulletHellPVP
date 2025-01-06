using UnityEngine;
using System;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    // probably should rename this script, especially once I finalize its functionality

    // Fields
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject manaBar;

    private byte CharacterIndex;
    [HideInInspector] public CharacterInfo OwnedCharacterInfo;
    [HideInInspector] public CharacterInfo OpponentCharacterInfo;
    [HideInInspector] public GameObject CharacterObject;

    // Methods
    public override void OnNetworkSpawn()
    {
        SetCharacterInfo((byte)OwnerClientId);
        if (!IsOwnedByServer)
        {
            MultiplayerManager.NonHostClientJoined = true;
        }
    }
    private void Start()
    {
        transform.parent.name = OwnedCharacterInfo.name;

        // Move health and mana bars to the right position
        healthBar.GetComponent<RectTransform>().localPosition = OwnedCharacterInfo.healthBarPos;
        manaBar.GetComponent<RectTransform>().localPosition = OwnedCharacterInfo.manaBarPos;
    }
    public void SetCharacterInfo(byte index)
    {
        Debug.Log($"char index {index} deleteme");

        CharacterIndex = index;

        OwnedCharacterInfo = GameSettings.Used.Characters[CharacterIndex];

        if (CharacterIndex == 0) OpponentCharacterInfo = GameSettings.Used.Characters[1];
        else OpponentCharacterInfo = GameSettings.Used.Characters[0];
    }
}
