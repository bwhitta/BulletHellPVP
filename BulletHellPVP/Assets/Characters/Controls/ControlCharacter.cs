using UnityEngine;
using UnityEngine.InputSystem;
using static ControlsManager;

public class ControlCharacter : MonoBehaviour
{
    private InputAction movementAction;
    private Rigidbody2D characterRigidbody;
    private CharacterInfo characterInfo;

    // Monobehavior methods
    private void Start()
    {
        SetObjectReferences();
        EnableMovement();
        gameObject.transform.position = characterInfo.CharacterStartLocation;
    }
    private void SetObjectReferences()
    {
        characterRigidbody = gameObject.GetComponent<Rigidbody2D>();
        characterInfo = gameObject.GetComponent<CharacterStats>().characterInfo;
    }

    private void EnableMovement()
    {
        // Debug.Log($"Enabling Input for character {characterInfo.name}");
        InputActionMap controllingMap = GetActionMap(characterInfo.InputMapName);
        movementAction = controllingMap.FindAction(characterInfo.MovementActionName, true);
        movementAction.Enable();
    }

    private void Update()
    {
        CheckCharacterMovement();
    }
    private void CheckCharacterMovement()
    {
        Vector2 movementVector = movementAction.ReadValue<Vector2>();

        characterRigidbody.velocity = characterInfo.DefaultStats.MovementSpeedMod * movementVector;

        gameObject.GetComponent<Animator>().SetFloat(characterInfo.AnimatorTreeParameterX, movementVector.x);
        gameObject.GetComponent<Animator>().SetFloat(characterInfo.AnimatorTreeParameterY, movementVector.y);
    }
}
