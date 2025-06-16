using UnityEngine;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    // Fields
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject manaBar;
    [SerializeField] private string[] characterParentObjectName;

    [HideInInspector] public byte CharacterIndex;
    public static Transform[] CharacterTransforms = new Transform[2];
    
    // Properties
    public string InputMapName => GameSettings.InputNames.InputMapNames[CharacterIndex];
    public byte OpponentCharacterIndex
    {
        get
        {
            if (CharacterIndex == 0) return 1;
            else return 0; // NEWLY ADJUSTED
        }
    }

    // Methods
    public override void OnNetworkSpawn()
    {
        CharacterIndex = (byte)OwnerClientId;
        
        if (!IsOwnedByServer)
        {
            MultiplayerManager.NonHostClientJoined = true;
        }
    }
    private void Start()
    {
        CharacterTransforms[CharacterIndex] = transform;
        
        transform.parent.name = characterParentObjectName[CharacterIndex];

        // Position stat bars
        healthBar.GetComponent<RectTransform>().localPosition = GameSettings.UIPositioning.HealthBars[CharacterIndex];
        manaBar.GetComponent<RectTransform>().localPosition = GameSettings.UIPositioning.ManaBars[CharacterIndex];
    }
}
