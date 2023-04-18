using Unity.Netcode;
using UnityEngine;

public class PlayerNetworking : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new();

    public override void OnNetworkSpawn()
    {
        Debug.Log($"GameObject {gameObject}. Owner {IsOwner}");
        Move();
    }

    public void Move()
    {
        Debug.Log($"Position value: {Position.Value}\nTransform position: {transform.position}");
        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = transform.position;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }
    [ServerRpc]
    private void SubmitPositionRequestServerRpc()
    {
        Position.Value = transform.position;
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            Debug.Log($"Syncing location to server position");
            transform.position = Position.Value;
        }
    }
}