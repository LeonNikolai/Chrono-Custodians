using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using System;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public UnityEvent onDeath;
    public UnityEvent onAlive;

    [SerializeField] private Material alive, dead;

    bool shouldUpdateHud = false;
    private void Awake() {
        if(onDeath == null) onDeath = new UnityEvent();
        if(onAlive == null) onAlive = new UnityEvent();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentHealth.Value = maxHealth.Value;
        }

        if(IsOwner) {
            if (TryGetComponent<Player>(out var player))
            {
                shouldUpdateHud = true;
            }
        }

        if (shouldUpdateHud)
        {
            Hud.Health = (float)currentHealth.Value / (float)maxHealth.Value;
        }
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


    bool _isDead = false;
    public bool IsDead
    {
        get
        {
            return IsDead;
        }
        private set
        {
            if (value == _isDead) return;
            _isDead = value;
            if (_isDead) Die(); else Alive();
        }
    }

    private void Alive()
    {
        onAlive?.Invoke();
    }

    public void TakeDamageServer(int damage)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        IsDead = newValue <= 0;
        if (shouldUpdateHud && IsOwner)
        {
            Hud.Health = (float)currentHealth.Value / (float)maxHealth.Value;
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
    }
}
