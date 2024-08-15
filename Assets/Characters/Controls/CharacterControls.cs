using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterControls : NetworkBehaviour
{
    // Fields
    [HideInInspector] public InputAction movementAction;
    private CharacterInfo characterInfo;
    private Animator characterAnimator;
    public class TempMovementMod
    {
        public Vector2 tempPush = Vector2.zero;
        public float tempMovementMod = 1f;
        public bool removeEffect = false;
        // public ushort lifetime; Not using for now, should instead change removeEffect from the SpellModuleBehavior
    }
    public List<TempMovementMod> tempMovementMods = new();
    
    // Monobehavior Methods
    private void Start()
    {
        Debug.LogWarning($"interpolation is currently removed! probably should re-add, delete me later.");

        SetObjectReferences();
        EnableMovement();
        transform.position = characterInfo.CharacterStartLocation;
        SetPositionOnline();

        // Local Methods
        void SetObjectReferences()
        {
            characterInfo = GetComponent<CharacterStats>().characterInfo;
            characterAnimator = GetComponent<Animator>();
        }
        void EnableMovement()
        {
            InputActionMap controllingMap = ControlsManager.GetActionMap(characterInfo.InputMapName);
            movementAction = controllingMap.FindAction(characterInfo.MovementActionName, true);
            movementAction.Enable();
        }
        void SetPositionOnline()
        {
            if (IsServer)
            {
                LocationUpdateClientRpc(transform.position);
            }
        }
    }
    private void FixedUpdate()
    {
        MovementTick();
        if (IsServer && IsOwner)
        {
            LocationUpdateClientRpc(transform.position);
        }
    }

    // Methods
    private void MovementTick()
    {
        Vector2 movementInput = movementAction.ReadValue<Vector2>();

        // Online Multiplayer
        if (MultiplayerManager.IsOnline && IsOwner)
        {
            OwnerMovementTick();
        }

        // Local multiplayer
        if (MultiplayerManager.multiplayerType == MultiplayerManager.MultiplayerTypes.Local)
        {
            MoveCharacter(movementInput);
        }

        // Counts down the remaining time on temporary movement effects
        TempMovementTick();

        // Local Methods
        void OwnerMovementTick()
        {
            if (IsHost)
            {
                MoveCharacter(movementInput);
            }
            else if (IsClient)
            {
                MoveCharacter(movementInput);
                MoveCharacterServerRpc(movementInput, transform.position);
            }
        }
        void TempMovementTick()
        {
            for (ushort i = 0; i < tempMovementMods.Count; i++)
            {
                if (tempMovementMods[i].removeEffect)
                {
                    tempMovementMods.RemoveAt(i);
                }
            }
        }
    }
    private void MoveCharacter(Vector2 movementInput)
    {
        Vector3 movement = GameSettings.Used.CharacterMovementSpeed * CalculateTempMovementMod() * movementInput.normalized;

        transform.position += (movement + (Vector3)CalculateTempPush()) * Time.fixedDeltaTime;

        characterAnimator.SetFloat(characterInfo.AnimatorTreeParameterX, movementInput.x);
        characterAnimator.SetFloat(characterInfo.AnimatorTreeParameterY, movementInput.y);

        // Local Methods
        float CalculateTempMovementMod(){
            float movementMod = 1f;
            foreach (TempMovementMod tempMod in tempMovementMods)
            {
                movementMod *= tempMod.tempMovementMod;
            }
            return movementMod;
        }
        Vector2 CalculateTempPush()
        {
            Vector2 movementMod = Vector2.zero;
            foreach (TempMovementMod tempMod in tempMovementMods)
            {
                movementMod += tempMod.tempPush;
            }
            return movementMod;
        }
    }
    
    // Rpcs
    [ClientRpc]
    private void LocationUpdateClientRpc(Vector2 pos)
    {
        if (IsHost) return;
        transform.position = pos;
    }
    [ServerRpc]
    private void MoveCharacterServerRpc(Vector2 inputVector, Vector2 clientPosition)
    {
        MoveCharacter(inputVector);

        Vector2 discrepancy = clientPosition - (Vector2)transform.position;
        if (discrepancy.magnitude >= GameSettings.Used.NetworkLocationDiscrepancyLimit)
        {
            Debug.Log($"{name} has a discrepancy of {discrepancy}");
            FixDiscrepancyClientRpc(discrepancy);
        }
    }
    [ClientRpc]
    private void FixDiscrepancyClientRpc(Vector2 discrepancy)
    {
        Debug.Log($"Client location wrong (discrepancy {discrepancy}).");
        if (IsOwner)
        {
            transform.position -= (Vector3)discrepancy;
        }
    }
}