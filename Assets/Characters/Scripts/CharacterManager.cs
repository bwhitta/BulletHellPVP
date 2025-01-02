using UnityEngine;
using System;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    // probably should rename this script, especially once I finalize its functionality

    // Fields
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject manaBar;

    [HideInInspector] public byte CharacterIndex;
    [HideInInspector] public CharacterInfo OwnedCharacterInfo;
    [HideInInspector] public CharacterInfo OpponentCharacterInfo;
    [HideInInspector] public GameObject CharacterObject;

    // Methods
    private void Start()
    {
        if (MultiplayerManager.IsOnline)
        {
            if ((IsHost && IsOwner) || (!IsHost && !IsOwner))
            {
                SetCharacterInfo(0);
            }
            else
            {
                SetCharacterInfo(1);
            }
        }

        name = OwnedCharacterInfo.name;

        // Move health and mana bars to the right position
        healthBar.GetComponent<RectTransform>().localPosition = OwnedCharacterInfo.healthBarPos;
        manaBar.GetComponent<RectTransform>().localPosition = OwnedCharacterInfo.manaBarPos;
    }
    public void SetCharacterInfo(byte index)
    {
        CharacterIndex = index;

        OwnedCharacterInfo = GameSettings.Used.Characters[CharacterIndex];

        if (CharacterIndex == 0) OpponentCharacterInfo = GameSettings.Used.Characters[1];
        else OpponentCharacterInfo = GameSettings.Used.Characters[0];
    }
}
