using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;

public class ServiceInitalizer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void Main()
    {
        var go = new GameObject("ServiceInitalizer");
        go.AddComponent<ServiceInitalizer>();
    }

    async void Start()
    {
        await InitalizeUnityServices();
    }
    public static async Task InitalizeUnityServices()
    {
        try
        {
            await UnityService();
            await Login();
            await VivoxService.Instance.InitializeAsync();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    public static async Task<bool> UnityService()
    {
        if (UnityServices.Instance.State == ServicesInitializationState.Initialized)
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
            Debug.Log("Unity Services Not Initialized");
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