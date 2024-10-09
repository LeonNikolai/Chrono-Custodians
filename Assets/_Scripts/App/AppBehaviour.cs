using System;
using System.Collections;
using UnityEngine;

// This class is used by the App class to do things that are not possible in a static class
[DefaultExecutionOrder(-100)]
public class AppBehaviour : MonoBehaviour
{
    [Serializable]
    struct asdasd
    {
        public int a;
        public int b;
    }
    private void OnDestroy()
    {
        if (App.IsRunning)
        {
            // Recreate the AppBehaviour if it is destroyed 
            // to make sure the App.OnApplicationQuit is called
            AppBootstraper.CreateAppBehaviour();
        }
    }

    private IEnumerator Start()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        // Check microphone permission
        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("Microphone found");
        }
        else
        {
            Debug.Log("Microphone not found");
        }
    }

    private void OnApplicationQuit()
    {
        // Because "Application.quitting" is not reliable in the editor
        // we put this in a monobehaviour to make sure it is called
        App.OnApplicationEnd();
    }
}