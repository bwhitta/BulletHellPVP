using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static ControlsManager;

// 249, 264 (margins around image)

public class ConsumableBarLogic : MonoBehaviour
{
        [Header("Character")]
    [SerializeField] private GameObject characterObject;
    [SerializeField] private ControlCharacter character;

    enum Stats { health, mana }
        [Header("Stats")]
    [SerializeField] Stats statToModify = new();
    [SerializeField] private GameObject statRemainingObject, statLostObject;
    [SerializeField] private int remainingEdgeLeft, remainingEdgeRight, lostEdgeLeft, lostEdgeRight;
    [SerializeField] private Text valueText;

        [Header("Stat loss bar")]
    private float statLost;
    private float statLostVelocity = 0;
    [SerializeField] private float statLostVelocityMod;
    

    private void Start()
    {
        character = characterObject.GetComponent<ControlCharacter>();
        InitialStatDisplay();
        UpdateText();
    }
    private void FixedUpdate()
    {
        UpdateStatLost();
    }

    public void ModifyStat(float statChange)
    {
        SetCurrentStat(GetCurrentStat() + statChange);
        UpdateStatDisplay(statRemainingObject, GetCurrentStat(), remainingEdgeLeft, remainingEdgeRight);
    }
    private void UpdateStatDisplay(GameObject displayObject, float valueSet, int edgeLeft = 0, int edgeRight = 0)
    {
        float statMax = GetStatMax();
        
        Image displayImage = displayObject.GetComponent<Image>();
        
        // Update divider bar
        float statPercentage = valueSet / statMax;
        float divMin = (-displayImage.preferredWidth / 2) + edgeLeft; // The left-most point of the bar
        float divLocation = divMin + ((displayImage.preferredWidth - (edgeLeft + edgeRight)) * statPercentage); // The divider's X along the bar


        GameObject div = displayImage.transform.GetChild(0).gameObject; // Divider game object
        div.GetComponent<RectTransform>().anchoredPosition = new Vector2(divLocation, 0); // Moves the divider into location
        displayImage.fillAmount = statPercentage;
    }

    private void InitialStatDisplay()
    {
        statLost = GetStatMax();
        UpdateStatDisplay(statRemainingObject, GetCurrentStat(), remainingEdgeLeft, remainingEdgeRight);
        UpdateStatDisplay(statLostObject, statLost, lostEdgeLeft, lostEdgeRight);
    }

    private void UpdateStatLost()
    {
        if (statLost > GetCurrentStat()) // Checks if 
        {
            statLost -= statLostVelocity; // Move healthAtLost towards healthRemaining
            if (statLost <= GetCurrentStat()) // If it's at or under healthRemaining,
            {
                statLost = GetCurrentStat(); // Floor at healthRemaining
                statLostVelocity = 0; // Stop velocity for next health loss
            }
            else
            {
                statLostVelocity += statLostVelocityMod; // Speed up velocity
            }
            UpdateStatDisplay(statLostObject, statLost, lostEdgeLeft, lostEdgeRight);
        }
        else if (statLost < GetCurrentStat())
        {
            statLost = GetCurrentStat(); // Floor at healthRemaining
            UpdateStatDisplay(statLostObject, statLost, lostEdgeLeft, lostEdgeRight);
        }
    }
    
    private float GetStatMax()
    {
        float statMax = 0;
        if (statToModify == Stats.health)
        {
            statMax = character.stats.MaxHealthStat;
        }
        else if (statToModify == Stats.mana)
        {
            statMax = character.stats.MaxManaStat;
        }
        return statMax;
    }
    private float GetCurrentStat()
    {
        float CurrentStat = 0;
        if (statToModify == Stats.health)
        {
            CurrentStat = character.stats.CurrentHealthStat;
        }
        else if (statToModify == Stats.mana)
        {
            CurrentStat = character.stats.CurrentManaStat;
        }
        else
        {
            Debug.LogError("No valid stat assigned");
        }
        return CurrentStat;
    }
    private void SetCurrentStat(float targetValue)
    {
        if (statToModify == Stats.health)
        {
            character.stats.CurrentHealthStat = targetValue;
        }
        else if (statToModify == Stats.mana)
        {
            character.stats.CurrentManaStat = targetValue;
        }
        else
        {
            Debug.LogError("No valid stat assigned");
        }
        UpdateText();
    }

    private void UpdateText()
    {
        if (statToModify == Stats.health)
        {
            valueText.text = character.stats.CurrentHealthStat.ToString();
        }
        else if (statToModify == Stats.mana)
        {
            valueText.text = character.stats.CurrentManaStat.ToString();
        }
    }
}