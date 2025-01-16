using UnityEngine;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    // probably should rename this script, especially once I finalize its functionality

    // Fields
    public string[] SortingLayers; // Maybe add as part of another script?

    [SerializeField] private GameObject healthBar;
    [SerializeField] private Vector2[] healthBarPositions;
    [SerializeField] private GameObject manaBar;
    [SerializeField] private Vector2[] manaBarPositions;

    [SerializeField] private string[] inputMapNames;
    [SerializeField] private string[] characterParentObjectName;

    [HideInInspector] public byte CharacterIndex;

    // Properties
    public string InputMapName => inputMapNames[CharacterIndex];
    public byte OpponentCharacterIndex
    {
        get
        {
            if (CharacterIndex == 0) return 1;
            else return 2;
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
        transform.parent.name = characterParentObjectName[CharacterIndex];

        // Position stat bars
        healthBar.GetComponent<RectTransform>().localPosition = healthBarPositions[CharacterIndex];
        manaBar.GetComponent<RectTransform>().localPosition = manaBarPositions[CharacterIndex];
    }
}
