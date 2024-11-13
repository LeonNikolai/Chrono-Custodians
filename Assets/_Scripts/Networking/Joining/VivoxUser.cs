using System;
using Unity.Netcode;
using Unity.Services.Vivox;
using Unity.Services.Vivox.AudioTaps;
using UnityEngine;

public class VivoxUser : NetworkBehaviour
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] VivoxParticipantTap participantTap;
    [SerializeField] VivoxCaptureSourceTap captureSourceTap;

    void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (participantTap == null) participantTap = GetComponent<VivoxParticipantTap>();
        if (captureSourceTap == null) captureSourceTap = GetComponent<VivoxCaptureSourceTap>();
        audioSource.enabled = false;
        participantTap.enabled = false;
        captureSourceTap.enabled = false;
    }
    public override void OnNetworkSpawn()
    {
        if (OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            var obj = FindAnyObjectByType<VivoxManager>();
            if (obj == null)
            {
                new GameObject("VivoxManager").AddComponent<VivoxManager>();
            }
        }
    }
}
