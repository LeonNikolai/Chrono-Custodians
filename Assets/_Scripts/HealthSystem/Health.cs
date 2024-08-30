using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<int> HealthValue = new NetworkVariable<int>(100);

    [SerializeField] private UnityEvent onDeath;

    [SerializeField] private Material alive, dead;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
            HealthValue.OnValueChanged += OnHealthChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            HealthValue.OnValueChanged -= OnHealthChanged;
        }
        base.OnNetworkDespawn();
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer) return;
        HealthValue.Value = Mathf.Max(HealthValue.Value-damage, 0);

        if (HealthValue.Value <= 0)
        {
            Die();
        }
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {

    }

    private void Die()
    {
        onDeath.Invoke();
    }

    public void testMethod()
    {
        GetComponent<MeshRenderer>().material = dead;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            TakeDamageServerRpc(100);
        }
    }
}
