using UnityEngine;
using UnityEngine.InputSystem;
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
        gameObject.transform.position = playerInfo.CharacterStartLocation;
    }
    private void SetObjectReferences()
    {
        playerRigidbody = gameObject.GetComponent<Rigidbody2D>();
        playerInfo = gameObject.GetComponent<CharacterStats>().playerInfo;
    }

    private void EnableMovement()
    {
        InputActionMap controllingPlayerMap = GetActionMap(playerInfo.InputMapName);
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

        gameObject.GetComponent<Animator>().SetFloat(playerInfo.AnimatorTreeParameterX, movementVector.x);
        gameObject.GetComponent<Animator>().SetFloat(playerInfo.AnimatorTreeParameterY, movementVector.y);
    }
}
