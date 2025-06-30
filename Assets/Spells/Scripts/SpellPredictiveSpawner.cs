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
        NetworkManager.PrefabHandler.AddHandler(prefab, this);
    }
    public override void OnNetworkDespawn()
    {
        NetworkManager.PrefabHandler.RemoveHandler(prefab);
    }

    public GameObject ClientSpawnSpellObject(Vector3 position, Quaternion rotation)
    {
        Debug.Log($"predictive spawning spell");
        NetworkObject spawnedObject = Instantiate(prefab, position, rotation);
        spawnedObject.SetSceneObjectStatus(false);
        queuedInstances.Enqueue(spawnedObject);
        return spawnedObject.gameObject;
    }
    public GameObject ServerSpawnSpellObject(ulong clientId, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"spawning spell on server");
        NetworkObject spawnedObject = NetworkManager.SpawnManager.InstantiateAndSpawn(prefab, clientId, true, false, false, position, rotation);
        return spawnedObject.gameObject;
    }

    // I assume these are called when a networkObject is created or destroyed
    NetworkObject INetworkPrefabInstanceHandler.Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        Debug.Log("handling instantiation!");
        NetworkObject instantiatedObject;
        if (NetworkManager.LocalClientId == ownerClientId)
        {
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
        Destroy(networkObject.gameObject);
    }
}