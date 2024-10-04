using UnityEngine;

public class TeleportInteraction : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] Transform target;
    [SerializeField] Vector3 fallBackPositonIfNoTarget;
    [SerializeField] Vector3 teleportOffset = Vector3.zero;

    public bool Interactible => true;
    public void Interact(Player player)
    {
        Vector3 position = target ? target.position : fallBackPositonIfNoTarget;
        player.Movement.ChangePosition(position + teleportOffset);
    }


    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position + teleportOffset, 0.5f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(fallBackPositonIfNoTarget, 0.3f);
    }
}