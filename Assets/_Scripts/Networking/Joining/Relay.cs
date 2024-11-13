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
            await ServiceInitalizer.Login();
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections: 10);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var data = allocation.ToRelayServerData("dtls");
            transport.SetRelayServerData(data);
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
            await ServiceInitalizer.Login();
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
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