using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(100);
    public NetworkVariable<int> currentHealth;

    [SerializeField] private UnityEvent onDeath;

    [SerializeField] private Material alive, dead;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsClient)
        {
            currentHealth.OnValueChanged += OnHealthChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            currentHealth.OnValueChanged -= OnHealthChanged;
        }
        base.OnNetworkDespawn();
    }

    [ServerRpc]
    public void TakeDamageServerRpc(int damage)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Max(currentHealth.Value-damage, 0);

        if (currentHealth.Value <= 0)
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
