using UnityEngine;

public enum LocationType
{
    Outside = 0,
    Inside = 1,
}
public class TeleportInteraction : MonoBehaviour, ILongInteractable, IInteractionMessage
{
    [Header("Teleport Settings")]
    [SerializeField] Transform target;
    [SerializeField] Vector3 fallBackPositonIfNoTarget;
    [SerializeField] Vector3 teleportOffset = Vector3.zero;
    [SerializeField] LocationType locationType = LocationType.Outside;
    [SerializeField] private string interactionMessage;
    [SerializeField] private float _interactTime = 1.5f;

    public float InteractTime => _interactTime;

    public string InteractionMessage => interactionMessage;

    public string CantInteractMessage => "Cannot interact";

    public void LongInteract(Player player)
    {
        if (target)
        {
            player.Movement.ChangePositionAndRotation(target.position + teleportOffset, target.rotation);
        }
        else
        {
            Vector3 position = target ? target.position : fallBackPositonIfNoTarget;
            player.Movement.ChangePosition(position + teleportOffset);
        }
        player.Location = locationType;
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