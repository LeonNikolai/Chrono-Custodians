using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class HealthSystem : NetworkBehaviour
{
    public NetworkVariable<int> maxHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float healthPercentage;
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
        if(newValue)
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerDead");
        } else if (!newValue)
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
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

    public void FullHeal()
    {
        if (!IsServer) return;
        currentHealth.Value = maxHealth.Value;
    }

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (IsServer) isDead.Value = newValue <= 0;
        if (shouldUpdateHud && IsOwner)
        {
            healthPercentage = (float)currentHealth.Value / (float)maxHealth.Value;
            Hud.Health = healthPercentage;
            if (previousValue > newValue)
            {
                TriggerDamageEffect(previousValue, newValue);
            }
        }
    }


    [SerializeField] private Volume playerPostProcessing;
    private float curIntensity = 0;
    private float damageEffectDuration = 0.8f;
    private Coroutine fadeOutRoutine;
    private Vignette vignette;


    private void TriggerDamageEffect(int previousValue, int newValue)
    {
        if (!IsOwner) return;
        if (playerPostProcessing == null)
        {
            Debug.Log("No Player volume");
            return;
        }
        float damageAmount = previousValue - newValue;
        float intensity = damageAmount / (float)currentHealth.Value;
        float minIntensity = Mathf.Clamp(1 - healthPercentage, 0, 0.6f);
        intensity = Mathf.Clamp(intensity += minIntensity, 0, 0.8f);
        EffectFadeIn(intensity, minIntensity);
    }

    private void EffectFadeIn(float intensity, float minIntensity)
    {
        if (fadeOutRoutine != null)
        {
            StopCoroutine(fadeOutRoutine);
        }
        if (vignette == null)
        {
            playerPostProcessing.profile.TryGet<Vignette>(out vignette);
        }
        StartCoroutine(DamageEffect(intensity, minIntensity));
    }
    private IEnumerator DamageEffect(float intensity, float minIntensity)
    {
        float elapsed = 0f;

        while (elapsed < 0.1f)
        {
            elapsed += Time.deltaTime;
            vignette.intensity.Override(Mathf.Lerp(curIntensity, intensity, elapsed * 10));
            yield return null;
        }
        elapsed = 0;

        while (elapsed < damageEffectDuration)
        {
            elapsed += Time.deltaTime;
            vignette.intensity.Override(Mathf.Lerp(curIntensity, minIntensity, elapsed / damageEffectDuration));
            yield return null;
        }
        curIntensity = minIntensity;
    }
}
