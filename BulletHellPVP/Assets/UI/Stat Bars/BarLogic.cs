using UnityEngine;
using UnityEngine.UI;

// 249, 264 (margins around image)

public class BarLogic : MonoBehaviour
{
    // Info about the character
    [SerializeField] private CharacterInfo characterInfo;

    public enum Stats { health, mana }

        [Header("Stats")]
    public Stats statToModify;
    [SerializeField] private GameObject statRemainingObject, statLostObject;
    [SerializeField] private int remainingEdgeLeft, remainingEdgeRight, lostEdgeLeft, lostEdgeRight;
    [SerializeField] private Text valueTextObject;
    
    // Stat loss bar
    private float statLost;
    private float statLostVelocity = 0;

    // Gets and/or sets the correct value from CharacterStatsScript
    private float StatRemaining
    {
        get
        {
            if (characterInfo.CharacterObject == null)
            {
                return 0.0f;
            }
            else if (statToModify == Stats.health)
            {
                return characterInfo.CharacterStatsScript.CurrentHealthStat;
            }
            else if (statToModify == Stats.mana)
            {
                return characterInfo.CharacterStatsScript.CurrentManaStat;
            }
            else
            {
                Debug.LogWarning("Invalid stat assigned!");
                return 0f;
            }
        }
    }
    private float StatMax
    {
        get
        {
            if (statToModify == Stats.health)
            {
                return characterInfo.defaultStats.MaxHealthStat;
            }
            else if (statToModify == Stats.mana)
            {
                return characterInfo.defaultStats.MaxManaStat;
            }
            else
            {
                Debug.LogError("Invalid stat assigned!");
                return 0f;
            }
        }
    }

    private void Awake()
    {
        gameObject.tag = characterInfo.CharacterTag;
    }
    private void Start()
    {
        if (characterInfo.CharacterObject == null)
        {
            BarEnabled(false);
        }
    }
    public void BarEnabled(bool enable)
    {
        if (enable == gameObject.activeSelf)
        {
            return;
        }
        gameObject.SetActive(enable);

        // Enabled Behavior
        if (enable)
        {
            statLost = StatRemaining;
            UpdateStatDisplays(UpdatableStats.Both);
        }
        // Disabled Behavior
        else
        {
            if(statToModify == Stats.health)
            {
                characterInfo.HealthBar = gameObject.GetComponent<BarLogic>();
            }
            else
            {
                characterInfo.ManaBar = gameObject.GetComponent<BarLogic>();
            }
        }
    }

    public enum UpdatableStats { Remaining, Lost, Both }
    public void UpdateStatDisplays(UpdatableStats updatedDisplays)
    {
        // Update the text
        valueTextObject.text = Mathf.Floor(StatRemaining).ToString();

        // Variables
        float edgeLeft, edgeRight, valueSet;
        Image displayImage;
        // Set variables based on parameter
        if(updatedDisplays == UpdatableStats.Remaining)
        {
            edgeLeft = remainingEdgeLeft;
            edgeRight = remainingEdgeRight;
            displayImage = statRemainingObject.GetComponent<Image>();
            valueSet = StatRemaining;
        }
        else if(updatedDisplays == UpdatableStats.Lost)
        {
            edgeLeft = lostEdgeLeft;
            edgeRight = lostEdgeRight;
            displayImage = statLostObject.GetComponent<Image>();
            valueSet = statLost;
        }
        else
        {
            // For the "Both" parameter 
            UpdateStatDisplays(UpdatableStats.Remaining);
            UpdateStatDisplays(UpdatableStats.Lost);
            return;
        }
        
        float statPercentage = valueSet / StatMax;
        float divMin = (-displayImage.preferredWidth / 2) + edgeLeft;
        float divLocation = divMin + ((displayImage.preferredWidth - (edgeLeft + edgeRight)) * statPercentage);

        // Update divider bar
        GameObject div = displayImage.transform.GetChild(0).gameObject; // Divider game object
        div.GetComponent<RectTransform>().anchoredPosition = new Vector2(divLocation, 0); // Moves the divider into location
        
        displayImage.fillAmount = statPercentage;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            UpdateStatLost();
        }
    }
    private void UpdateStatLost()
    {
        // Checks if statLost is too high
        if (statLost > StatRemaining)
        {
            // Move statLost down and speed up velocity
            statLost -= statLostVelocity * Time.deltaTime;
            statLostVelocity += characterInfo.defaultStats.StatLostVelocityMod;

            UpdateStatDisplays(UpdatableStats.Lost);
        }
        // Check if statLost is now too low
        if (statLost < StatRemaining)
        {
            // Floor at actual stat and stop the movement
            statLost = StatRemaining; 
            statLostVelocity = 0;

            UpdateStatDisplays(UpdatableStats.Lost);
        }
    }
}