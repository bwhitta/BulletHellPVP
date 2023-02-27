using JetBrains.Annotations;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static ConsumableBarLogic;
using static ControlsManager;

public class ControlCharacter : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private string controllingPlayerMapName;
    [SerializeField] private string movementActionName;
    private InputAction movementAction;

    [Header("Movement")]
    [SerializeField] private float MovementSpeedMod; // Movement speed multiplier
    private Rigidbody2D playerRigidbody;

    [Header("Animation")]
    [SerializeField] private Animator CharacterAnimator; // The animator object with the animation tree use
    [SerializeField] private string AnimatorTreeParameterX, AnimatorTreeParameterY; // The names of the parameters for the animation tree

    // Monobehavior methods
    private void Start()
    {
        // Get rigidbody for use in Update
        playerRigidbody = gameObject.GetComponent<Rigidbody2D>();

        // Get and enable movement
        InputActionMap controllingPlayerMap = ControlsManager.GetActionMap(controllingPlayerMapName);
        movementAction = controllingPlayerMap.FindAction(movementActionName, true);
        movementAction.Enable();
    }
    private void Update()
    {
        CheckCharacterMovement();
    }
    private void CheckCharacterMovement()
    {
        Vector2 movementVector = movementAction.ReadValue<Vector2>(); // Reads the "movement" input action's vector

        playerRigidbody.velocity = MovementSpeedMod * movementVector; // Moves the characterStats

        CharacterAnimator.SetFloat("FacingX", movementVector.x); // Tells the animator to show the characterStats as facing left or right
        CharacterAnimator.SetFloat("FacingY", movementVector.y); // Tells the animator to show the characterStats as facing up or down
    }
}
