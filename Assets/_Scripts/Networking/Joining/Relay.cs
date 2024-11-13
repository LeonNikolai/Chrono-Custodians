using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Core;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;

public static class Relay
{
    public static async Task<(string joinCode, Allocation allocation)> StartHostAsync()
    {
        try
        {
            Debug.Log("Sign in anonymously");
            await ServiceInitalizer.Login();

            // Allocate a Relay server
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections: 10);

            Debug.Log($"Relay allocation created with ID: {allocation.AllocationId}");
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay join code: {joinCode}");
            // Configure Unity Transport with Relay data
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            var data = allocation.ToRelayServerData("dtls");
            transport.SetRelayServerData(data);
            Debug.Log($"Relay Server started with join code: {joinCode}");

            return (joinCode, allocation);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay allocation error: {e.Message}");
        }
        return (null, null);
    }

    public static async Task<JoinAllocation> JoinRelayAsync(string joinCode)
    {
        try
        {
            await ServiceInitalizer.InitalizeUnityServices();
            // Join an existing Relay allocation
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // Configure Unity Transport with Relay data
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            var data = allocation.ToRelayServerData("dtls");
            transport.SetRelayServerData(data);

            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay join error: {e.Message}");
        }
        return null;
    }
}