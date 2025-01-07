using UnityEngine;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    // probably should rename this script, especially once I finalize its functionality

    // Fields
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject manaBar;

    [HideInInspector] public CharacterInfo OwnerInfo; // Gonna try to lean a lot less on these
    [HideInInspector] public CharacterInfo OpponentInfo;
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
        transform.parent.name = OwnerInfo.name;

        // Position stat bars
        healthBar.GetComponent<RectTransform>().localPosition = OwnerInfo.HealthBarPos;
        manaBar.GetComponent<RectTransform>().localPosition = OwnerInfo.ManaBarPos;
    }
    public void SetCharacterInfo(byte index)
    {
        OwnerInfo = GameSettings.Used.Characters[index];

        if (index == 0) OpponentInfo = GameSettings.Used.Characters[1];
        else OpponentInfo = GameSettings.Used.Characters[0];
    }
}
