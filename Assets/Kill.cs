using UnityEngine;

public class Kill : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<HealthSystem>(out HealthSystem health))
        {
            health.TakeDamageServer(100000);
        }
    }
}
