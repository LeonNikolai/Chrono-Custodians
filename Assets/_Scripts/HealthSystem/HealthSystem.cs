using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public UnityEvent<bool> onDeath;

    [SerializeField] private Material alive, dead;

    bool shouldUpdateHud = false;
    private void Awake()
    {
        if (onDeath == null) onDeath = new();
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentHealth.Value = maxHealth.Value;
        }

        if (IsOwner)
        {
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
        isDead.OnValueChanged += OnDeath;
    }

    private void OnDeath(bool previousValue, bool newValue)
    {
        onDeath?.Invoke(newValue);
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            currentHealth.OnValueChanged -= OnHealthChanged;
        }
        isDead.OnValueChanged -= OnDeath;
        base.OnNetworkDespawn();
    }



    public bool IsDead
    {
        get
        {
            return isDead.Value;
        }
    }


    public void TakeDamageServer(int damage)
    {
        if (!IsServer) return;
        currentHealth.Value = Mathf.Max(currentHealth.Value - damage, 0);
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (IsServer) isDead.Value = newValue <= 0;
        if (shouldUpdateHud && IsOwner)
        {
            Hud.Health = (float)currentHealth.Value / (float)maxHealth.Value;
        }
    }
}
