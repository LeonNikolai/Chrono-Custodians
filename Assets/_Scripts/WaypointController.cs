using UnityEngine;

public class WaypointController : MonoBehaviour
{
    [SerializeField] public WaypointType type;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
