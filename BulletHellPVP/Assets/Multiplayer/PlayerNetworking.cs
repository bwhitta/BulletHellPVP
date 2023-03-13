using Unity.Netcode;
using UnityEngine;

public class PlayerNetworking : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new();

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Move();
        }
    }

    public void Move()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Position.Value = gameObject.transform.position;
        }
        else
        {
            SubmitPositionRequestServerRpc();
        }
    }

    [ServerRpc]
    private void SubmitPositionRequestServerRpc()
    {
        Position.Value = gameObject.transform.position;
    }

    private void Update()
    {
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Syncing location to server position");
            transform.position = Position.Value;
        }
        else
        {
            Debug.Log("Skipped server position sync because game is marked as Local.");
        }
    }
}