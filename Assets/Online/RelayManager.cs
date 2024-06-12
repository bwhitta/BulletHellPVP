using Unity.Services.Relay;
using UnityEngine;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance {  get; private set; }
    public static Allocation allocation;
    public static JoinAllocation joinAllocation;
    [SerializeField] private byte maxPlayers;
    public enum InstanceModes { Host, Client }
    public static InstanceModes LocalInstanceMode;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"It seems that there are either two relay managers or that RelayManager isn't nullable.");
        }   
        Instance = this;
    }
    
    // Relay
    public async Task<string> CreateRelay()
    {
        try
        {
            // Create the relay
            allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

            // Find its join code
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"joinCode: {joinCode}");
            
            // Store information on this instance then load the gameplay scene
            LocalInstanceMode = InstanceModes.Host;

            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }

    public async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            Debug.Log($"Joining relay with {joinCode}");
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            LocalInstanceMode = InstanceModes.Client;
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
            return null;
        }
    }
}
