using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// Based on a script from this thread: https://discussions.unity.com/t/client-predicted-spawning-basic-implementation-client-server-architecture/1540866
// Github link: https://github.com/Extrys/NGO-ClientPredictedSpawn/blob/main/Advanced/TestBlockPredSpawn.cs
public class SpellPredictiveSpawner : NetworkBehaviour, INetworkPrefabInstanceHandler
{
    // Fields
    [SerializeField] private NetworkObject prefab; // make sure this prefab is registered to the NetworkManager
    private Queue<NetworkObject> queuedInstances = new();

    public static SpellPredictiveSpawner Instance;

    // Methods
    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning($"Existing instance of this singleton already exists");
            Destroy(this);
        }
    }
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

    public GameObject  SpawnSpellObject(Vector3 position, Quaternion rotation)
    {
        // Spawns an object locally and asks the server to spawn it
        Debug.Log($"spawn time");
        NetworkObject spawnedObject = Instantiate(prefab, position, rotation);
        spawnedObject.SetSceneObjectStatus(false);
        Debug.Log($"enqueuing");
        queuedInstances.Enqueue(spawnedObject);
        Debug.Log($"calling spawn server rpc. queuedInstances: [{string.Join<NetworkObject>(',', queuedInstances.ToArray())}]. gameObject: {gameObject}", gameObject);
        SpawnServerRPC(NetworkManager.LocalClientId, position, rotation);

        return spawnedObject.gameObject;
    }

    //[ServerRpc(RequireOwnership = false)]
    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void SpawnServerRPC(ulong clientId, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"Spawning object");
        NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, clientId, false, false, false, position, rotation);
    }

    // I assume these are called when a networkObject is created or destroyed
    // this modifies the base network prefab spawning behavior. previously it would just 
    NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        NetworkObject instantiatedObject;
        if (NetworkManager.LocalClientId == ownerClientId)
        {
            Debug.Log($"dequeuing! queuedInstances: [{string.Join<NetworkObject>(',', queuedInstances.ToArray())}]. gameObject: {gameObject}", gameObject);
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