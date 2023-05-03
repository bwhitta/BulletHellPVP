using Mono.CompilerServices.SymbolWriter;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static ControlsManager;

public class CharacterControls : NetworkBehaviour
{
    #region References
    public InputAction movementAction;
    private CharacterInfo characterInfo;
    private Animator characterAnimator;
    #endregion

    #region Fields
    // Temporary variables, cleared after movement (IMPLEMENTATION IS CURRENTLY REMOVED - MAKE SURE TO ADD BACK IN ONCE MULTIPLAYER IS FUNCTIONAL)
    [HideInInspector] public Vector2 tempPush;
    [HideInInspector] public float tempMovementMod;

    private NetworkVariable<Vector2> serverSidePosition = new();
    private Vector2 previousServerSidePosition;
    private int ticksSincePositionUpdate;
    #endregion Fields

    #region Monobehavior Methods
    private void Start()
    {
        SetObjectReferences();
        EnableMovement();
        transform.position = characterInfo.CharacterStartLocation;
        SetPositionOnline();
        NetworkVariableListeners();

        // Local Methods
        void SetObjectReferences()
        {
            characterInfo = GetComponent<CharacterStats>().characterInfo;
            characterAnimator = GetComponent<Animator>();
        }
        void EnableMovement()
        {
            InputActionMap controllingMap = GetActionMap(characterInfo.InputMapName);
            movementAction = controllingMap.FindAction(characterInfo.MovementActionName, true);
            movementAction.Enable();
            tempMovementMod = 1;
        }
        void SetPositionOnline()
        {
            if (IsServer)
            {
                serverSidePosition.Value = transform.position;
            }
            if (IsClient && !IsOwner)
            {
                transform.position = serverSidePosition.Value;
            }
        }
        void NetworkVariableListeners()
        {
            if (!IsOwner)
            {
                serverSidePosition.OnValueChanged += ServerSideLocationUpdate;
            }
            
        }
        void ServerSideLocationUpdate(Vector2 prevLocation, Vector2 newLocation)
        {
            previousServerSidePosition = prevLocation;
            ticksSincePositionUpdate = 0;
        }
    }

    private void FixedUpdate()
    {
        FixedMovementTick();

        if (IsServer)
        {
            ServerPositionTick();
        }
    }
    #endregion

    #region Methods
    private void FixedMovementTick()
    {
        Vector2 movementInput = movementAction.ReadValue<Vector2>();
        
        if (IsOwner)
        {
            OwnerMovementTick();
        }
        else if (!IsServer)
        {
            OpponentTick();
        }
        // Local
        if (MultiplayerManager.multiplayerType == MultiplayerManager.MultiplayerTypes.Local)
        {
            MoveCharacter(movementInput);
        }

        // Local Methods
        void OwnerMovementTick()
        {
            if (IsHost)
            {
                Vector2 movementVector = MoveCharacter(movementInput);
            }
            else if (IsClient)
            {
                Vector2 movementVector = MoveCharacter(movementInput);
                MoveCharacterServerRpc(movementInput, transform.position);
            }
        }
        void OpponentTick()
        {
            ticksSincePositionUpdate++;
            float cappedTicks = Mathf.Min(ticksSincePositionUpdate, characterInfo.UsedGameSettings.ServerLocationTickFrequency);
            float interpolatePercent = cappedTicks / characterInfo.UsedGameSettings.ServerLocationTickFrequency;
            transform.position = Calculations.RelativeTo(previousServerSidePosition, serverSidePosition.Value, interpolatePercent);
        }
    }
    private void ServerPositionTick()
    {
        ticksSincePositionUpdate++;
        if (ticksSincePositionUpdate >= characterInfo.UsedGameSettings.ServerLocationTickFrequency)
        {
            serverSidePosition.Value = transform.position;

            ticksSincePositionUpdate = 0;
        }
    }
    public Vector2 MoveCharacter(Vector2 movementInput)
    {
        Vector2 movement = 2.5f * movementInput.normalized;
        
        transform.position += (Vector3)movement * Time.fixedDeltaTime;

        characterAnimator.SetFloat(characterInfo.AnimatorTreeParameterX, movementInput.x);
        characterAnimator.SetFloat(characterInfo.AnimatorTreeParameterY, movementInput.y);

        return movement;
    }
    #endregion
    #region Server and Client RPCs
    [ServerRpc]
    private void MoveCharacterServerRpc(Vector2 inputVector, Vector2 clientPosition)
    {
        MoveCharacter(inputVector);

        Vector2 discrepancy = clientPosition - (Vector2)transform.position;
        if (discrepancy.magnitude >= characterInfo.UsedGameSettings.ServerClientDiscrepancyLimit)
        {
            Debug.Log($"{name} has a discrepancy");
            UpdateLocationClientRpc(discrepancy);
        }
    }

    [ClientRpc]
    private void UpdateLocationClientRpc(Vector2 discrepancy)
    {
        Debug.Log($"Client location wrong (discrepancy {discrepancy}).");
        if (IsOwner)
        {
            transform.position -= (Vector3)discrepancy;
        }
    }
    #endregion
}
