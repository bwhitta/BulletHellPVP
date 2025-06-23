using Unity.Netcode;
using UnityEngine;

public class RegularSpawner : NetworkBehaviour
{
    // Fields
    [SerializeField] private NetworkObject prefab;
    
    // Methods
    private void Update()
    {
        // Detect when button is pressed
        if (UnityEngine.InputSystem.Mouse.current.forwardButton.wasPressedThisFrame)
        {
            Debug.Log($"Spawning object");
            SpawnObject(new Vector3(Random.Range(-5, 5), Random.Range(-5, 5)));
        }
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.PrefabHandler.RemoveHandler(prefab);
    }

    public void SpawnObject(Vector3 position)
    {
        Debug.Log($"spawn time");
        SpawnServerRPC(NetworkManager.LocalClientId, position);
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnServerRPC(ulong clientId, Vector3 pos)
    {
        prefab.InstantiateAndSpawn(NetworkManager, clientId, position: pos);
    }
}