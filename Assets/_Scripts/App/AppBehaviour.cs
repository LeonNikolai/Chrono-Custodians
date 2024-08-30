using System;
using UnityEngine;

// This class is used by the App class to do things that are not possible in a static class
[DefaultExecutionOrder(-100)]
public class AppBehaviour : MonoBehaviour
{
    [Serializable]
    struct asdasd {
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
    private void OnApplicationQuit()
    {
        // Because "Application.quitting" is not reliable in the editor
        // we put this in a monobehaviour to make sure it is called
        App.OnApplicationEnd();
    }
}