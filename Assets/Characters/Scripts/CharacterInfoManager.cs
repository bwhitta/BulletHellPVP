using UnityEngine;

public class CharacterInfoManager : MonoBehaviour
{
    [SerializeField] private CharacterInfo inspectorCharacterInfoLeft;
    [SerializeField] private CharacterInfo inspectorCharacterInfoRight;
    
    private static bool characterLeftJoined;
    private static bool characterRightJoined;
    
    private static CharacterInfo CharacterInfoLeft;
    private static CharacterInfo CharacterInfoRight;

    private void Awake()
    {
        ResetInfoManager();
    }

    private void ResetInfoManager(bool throwOnFailure = true)
    {
        ResetJoinedCharacters();
        ResetCharacterInfoStatics();

        void ResetJoinedCharacters()
        {
            characterLeftJoined = false;
            characterRightJoined = false;
        }
        void ResetCharacterInfoStatics()
        {
            if (throwOnFailure && (inspectorCharacterInfoLeft == null || inspectorCharacterInfoRight == null))
            {
                Debug.LogWarning("Character info in characterInfoManager null");
            }
            CharacterInfoLeft = inspectorCharacterInfoLeft;
            CharacterInfoRight = inspectorCharacterInfoRight;
        }
    }
    
    public static CharacterInfo JoinAvailableLocation()
    {
        //Debug.Log($"Character joining! Left location: {characterLeftJoined}, Right location: {characterRightJoined}");
        if(characterLeftJoined == false)
        {
            characterLeftJoined = true;
            return CharacterInfoLeft;
        }
        else if (characterRightJoined == false)
        {
            characterRightJoined = true;
            return CharacterInfoRight;
        }
        else
        {
            Debug.LogWarning("No available slot.");
            return null;
        }
    }
}
