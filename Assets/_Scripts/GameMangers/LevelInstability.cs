using System;
using Unity.Netcode;
using UnityEngine;

public class LevelStability : NetworkBehaviour
{
    public LevelScene scene;
    public AnimationCurve itemCountCurve = AnimationCurve.Linear(0, 10, 1, 1);
    public int GetItemCount()
    {
        return Mathf.RoundToInt(itemCountCurve.Evaluate(Stability / 100));
    }
    private void Awake()
    {
        LevelManager.AllScenes.Add(scene);
    }
    public NetworkVariable<float> _stability = new NetworkVariable<float>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> _visitCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public float Stability
    {
        get => _stability.Value;
        set
        {
            if (IsServer)
            {
                _stability.Value = Mathf.Clamp(value, 0, 100);
            }
        }
    }
    public float Instability => 100 - Stability;
    public float Visits => _visitCount.Value;
}