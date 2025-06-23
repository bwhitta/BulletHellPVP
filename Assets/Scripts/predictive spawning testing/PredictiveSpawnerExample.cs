using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Based on a script from this thread: https://discussions.unity.com/t/client-predicted-spawning-basic-implementation-client-server-architecture/1540866
// Github link: https://github.com/Extrys/NGO-ClientPredictedSpawn/blob/main/Advanced/TestBlockPredSpawn.cs
public class PredictiveSpawnerExample : NetworkBehaviour, INetworkPrefabInstanceHandler
{
    // Fields
    [SerializeField] private NetworkObject prefab; // make sure this prefab is registered to the NetworkManager
    private Queue<NetworkObject> queuedInstances = new();

    // Methods
    public override void OnNetworkSpawn()
    {
        // handles deletion
        NetworkManager.PrefabHandler.AddHandler(prefab, this);

        // if objects were created before this client joined, spawn them (I think)
        /*List<NetworkObject> offlineInstances = queuedInstances.ToList();
        foreach (NetworkObject instance in offlineInstances)
        {
            SpawnServerRPC(NetworkManager.LocalClientId, instance.transform.position);
        }*/
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.PrefabHandler.RemoveHandler(prefab);
    }

    public void SpawnObject(Vector3 position)
    {
        // Spawns an object locally and asks the server to spawn it
        Debug.Log($"spawn time");
        NetworkObject spawnedObject = Instantiate(prefab, position, new Quaternion()/*, orientation*/);
        spawnedObject.SetSceneObjectStatus(false);
        queuedInstances.Enqueue(spawnedObject);
        SpawnServerRPC(NetworkManager.LocalClientId, position/*, orientation*/);
    }

    [ServerRpc(RequireOwnership = false)] //El instantiate realmente esta siendo overriden por el metodo de abajo
    void SpawnServerRPC(ulong clientId, Vector3 position)
    {
        Debug.Log($"Spawning object");
        NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, clientId, false, false, false, position, new Quaternion()/*, orientation*/);
    }

    // I assume these are called when a networkObject is created or destroyed
    // this modifies the base network prefab spawning behavior. previously it would just 
    NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        NetworkObject instantiatedObject;
        if (NetworkManager.LocalClientId == ownerClientId)
        {
            // if the local 
            instantiatedObject = queuedInstances.Dequeue();
        }
        else
        {
            instantiatedObject = Instantiate(prefab, position, rotation);
        }

        // Everybody returns the spawned object on their end, which is then somehow linked up behind the scenes.
        return instantiatedObject;
    }
    void INetworkPrefabInstanceHandler.Destroy(NetworkObject networkObject)
    {
        Debug.Log($"Destroying object");
        Destroy(networkObject);
    }
}