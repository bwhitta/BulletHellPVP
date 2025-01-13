using UnityEngine;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    // probably should rename this script, especially once I finalize its functionality

    // Fields
    public string[] SortingLayers;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject manaBar;
    [SerializeField] private string[] inputMapNames;

    [HideInInspector] public byte CharacterIndex;
    [HideInInspector] public CharacterInfo OwnerInfo;
    [HideInInspector] public CharacterInfo OpponentInfo;
    [HideInInspector] public GameObject CharacterObject;

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
        SetCharacterInfo((byte)OwnerClientId);
        if (!IsOwnedByServer)
        {
            MultiplayerManager.NonHostClientJoined = true;
        }
    }
    private void Start()
    {
        transform.parent.name = OwnerInfo.name;

        // Position stat bars
        healthBar.GetComponent<RectTransform>().localPosition = OwnerInfo.HealthBarPosition;
        manaBar.GetComponent<RectTransform>().localPosition = OwnerInfo.ManaBarPosition;
    }
    public void SetCharacterInfo(byte index)
    {
        CharacterIndex = index;
        OwnerInfo = GameSettings.Used.Characters[index];

        
    }
}
