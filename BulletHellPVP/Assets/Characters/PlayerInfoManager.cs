using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerInfoManager : MonoBehaviour
{
    [Header("Player info for reference from other scripts")]
    [SerializeField] private PlayerInfo inspectorPlayerInfoLeft;
    [SerializeField] private PlayerInfo inspectorPlayerInfoRight;
    
    private static bool playerLeftJoined;
    private static bool playerRightJoined;

    private static PlayerInfo PlayerInfoLeft;
    private static PlayerInfo PlayerInfoRight;

    private void Awake()
    {
        ResetInfoManager();
    }

    private void ResetInfoManager(bool throwOnFailure = true)
    {
        ResetJoinedPlayers();
        ResetPlayerInfoStatics();

        void ResetJoinedPlayers()
        {
            playerLeftJoined = false;
            playerRightJoined = false;
        }
        void ResetPlayerInfoStatics()
        {
            if (throwOnFailure && (inspectorPlayerInfoLeft == null || inspectorPlayerInfoRight == null))
            {
                Debug.LogWarning("Player info in playerInfoManager null");
            }
            PlayerInfoLeft = inspectorPlayerInfoLeft;
            PlayerInfoRight = inspectorPlayerInfoRight;
        }
    }
    
    public static PlayerInfo JoinAvailableLocation()
    {
        //Debug.Log($"Player joining! Left location: {playerLeftJoined}, Right location: {playerRightJoined}");
        if(playerLeftJoined == false)
        {
            playerLeftJoined = true;
            return PlayerInfoLeft;
        }
        else if (playerRightJoined == false)
        {
            playerRightJoined = true;
            return PlayerInfoRight;
        }
        else
        {
            Debug.LogWarning("No available slot.");
            return null;
        }
    }
}
