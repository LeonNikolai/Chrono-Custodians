using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
[DefaultExecutionOrder(-1000)]
public class ServiceInitalizer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Main()
    {
        var go = new GameObject("ServiceInitalizer");
        go.AddComponent<ServiceInitalizer>();
    }

    async void Start()
    {
        await InitalizeUnityServices();
        await InitalizeVivoxServices();
    }

    public static async Task InitalizeUnityServices()
    {
        try
        {
            await UnityService();
            await Login();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

    }

    public static bool VivoxInitialized { get; private set; } = false;
    public static async Task<bool> InitalizeVivoxServices()
    {
        if (VivoxInitialized)
        {
            return true;
        }
        try
        {
            await InitalizeUnityServices();
            await VivoxService.Instance.InitializeAsync();
            VivoxInitialized = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }

    public static async Task<bool> UnityService()
    {
        if (UnityServices.Instance.State != ServicesInitializationState.Uninitialized)
        {
            return true;
        }
        try
        {
            await UnityServices.InitializeAsync();
            if (UnityServices.Instance.State != ServicesInitializationState.Initialized)
            {
                Debug.Log("Failed To Initialize Unity Services");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }
    public static async Task<bool> Login()
    {
        if (!await UnityService())
        {
            return false;
        }
        if (AuthenticationService.Instance.IsSignedIn)
        {
            return true;
        }
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Failed To sign in");
                return false;
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            return false;
        }
    }
}