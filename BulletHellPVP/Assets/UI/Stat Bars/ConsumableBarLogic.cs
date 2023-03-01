using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private CharacterStats characterStats;

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

    // Gets and/or sets the correct value from CharacterStats
    private float StatRemaining
    {
        get
        {
            if (statToModify == Stats.health)
            {
                return characterStats.CurrentHealthStat;
            }
            else if (statToModify == Stats.mana)
            {
                return characterStats.CurrentManaStat;
            }
            else
            {
                Debug.LogError("Invalid stat assigned!");
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
                return characterStats.MaxHealthStat;
            }
            else if (statToModify == Stats.mana)
            {
                return characterStats.MaxManaStat;
            }
            else
            {
                Debug.LogError("Invalid stat assigned!");
                return 0f;
            }
        }
    }

    // Monobehavior Methods
    private void Start()
    {
        characterStats = characterObject.GetComponent<CharacterStats>();

        statLost = StatRemaining;
        UpdateStatDisplay(UpdatableStats.Both);
    }
    private void FixedUpdate()
    {
        UpdateStatLost();
    }

    public enum UpdatableStats { Remaining, Lost, Both }
    public void UpdateStatDisplay(UpdatableStats updatedStat)
    {
        // Update the text
        valueText.text = Mathf.Floor(StatRemaining).ToString();

        // Variables
        float edgeLeft, edgeRight, valueSet;
        Image displayImage;
        // Set variables based on parameter
        if(updatedStat == UpdatableStats.Remaining)
        {
            edgeLeft = remainingEdgeLeft;
            edgeRight = remainingEdgeRight;
            displayImage = statRemainingObject.GetComponent<Image>();
            valueSet = StatRemaining;
        }
        else if(updatedStat == UpdatableStats.Lost)
        {
            edgeLeft = lostEdgeLeft;
            edgeRight = lostEdgeRight;
            displayImage = statLostObject.GetComponent<Image>();
            valueSet = statLost;
        }
        else
        {
            // For the "Both" parameter 
            UpdateStatDisplay(UpdatableStats.Remaining);
            UpdateStatDisplay(UpdatableStats.Lost);
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

    private void UpdateStatLost()
    {
        // Checks if statLost is too high
        if (statLost > StatRemaining)
        {
            // Move statLost down and speed up velocity
            statLost -= statLostVelocity;
            statLostVelocity += statLostVelocityMod;

            UpdateStatDisplay(UpdatableStats.Lost);
        }
        // Check if statLost is now too low
        if (statLost < StatRemaining)
        {
            // Floor at actual stat and stop the movement
            statLost = StatRemaining; 
            statLostVelocity = 0;

            UpdateStatDisplay(UpdatableStats.Lost);
        }
    }
}