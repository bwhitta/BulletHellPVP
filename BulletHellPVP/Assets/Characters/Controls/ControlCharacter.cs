using UnityEngine;
using UnityEngine.InputSystem;
using static ControlsManager;

public class ControlCharacter : MonoBehaviour
{
    private InputAction movementAction;
    private Rigidbody2D characterRigidbody;
    private CharacterInfo characterInfo;
    public Vector2 InputVector
    {
        get
        {
            return movementAction.ReadValue<Vector2>();
        }
    }

    // Temporary variables, cleared after movement
    public Vector2 tempPush;
    public float tempMovementMod;

    // Monobehavior methods
    private void Start()
    {
        SetObjectReferences();
        EnableMovement();
        gameObject.transform.position = characterInfo.CharacterStartLocation;

        void EnableMovement()
        {
            InputActionMap controllingMap = GetActionMap(characterInfo.InputMapName);
            movementAction = controllingMap.FindAction(characterInfo.MovementActionName, true);
            movementAction.Enable();
        }
    }
    private void SetObjectReferences()
    {
        characterRigidbody = gameObject.GetComponent<Rigidbody2D>();
        characterInfo = gameObject.GetComponent<CharacterStats>().characterInfo;
    }

    private void LateUpdate()
    {
        //Debug.Log(InputVector);
        MoveCharacter();
        void MoveCharacter()
        {
            float movementMod = characterInfo.DefaultStats.MovementSpeedMod * tempMovementMod;
            characterRigidbody.velocity = movementMod * InputVector;
            characterRigidbody.velocity += tempPush;

            gameObject.GetComponent<Animator>().SetFloat(characterInfo.AnimatorTreeParameterX, InputVector.x);
            gameObject.GetComponent<Animator>().SetFloat(characterInfo.AnimatorTreeParameterY, InputVector.y);
        }

        tempPush = Vector2.zero;
        tempMovementMod = 1;
    }
}
