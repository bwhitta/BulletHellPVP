using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using static BarLogic;
using static ControlsManager;

public class ControlCharacter : MonoBehaviour
{
    private InputAction movementAction;
    private Rigidbody2D playerRigidbody;
    private PlayerInfo playerInfo;

    // Monobehavior methods
    private void Start()
    {
        SetObjectReferences();
        EnableMovement();
    }
    private void SetObjectReferences()
    {
        playerRigidbody = gameObject.GetComponent<Rigidbody2D>();
        playerInfo = gameObject.GetComponent<CharacterStats>().playerInfo;
    }

    private void EnableMovement()
    {
        InputActionMap controllingPlayerMap = GetActionMap(playerInfo.ControllingMap);
        movementAction = controllingPlayerMap.FindAction(playerInfo.MovementActionName, true);
        movementAction.Enable();
    }

    private void Update()
    {
        CheckCharacterMovement();
    }
    private void CheckCharacterMovement()
    {
        Vector2 movementVector = movementAction.ReadValue<Vector2>();

        playerRigidbody.velocity = playerInfo.defaultStats.MovementSpeedMod * movementVector;

        playerInfo.CharacterAnimator.SetFloat(playerInfo.AnimatorTreeParameterX, movementVector.x);
        playerInfo.CharacterAnimator.SetFloat(playerInfo.AnimatorTreeParameterY, movementVector.y);
    }
}
