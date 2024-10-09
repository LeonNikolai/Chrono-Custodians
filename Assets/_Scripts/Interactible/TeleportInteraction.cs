using UnityEngine;

public class TeleportInteraction : MonoBehaviour, IInteractable
{
    [Header("Teleport Settings")]
    [SerializeField] Transform target;
    [SerializeField] Vector3 fallBackPositonIfNoTarget;
    [SerializeField] Vector3 teleportOffset = Vector3.zero;

    [SerializeField] bool toggleInside = false;

    private float reflectionIntensity = 1;

    public bool Interactible => true;
    public void Interact(Player player)
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

        // We should not do this but fuck you all I'm doing it anywayyyyyyy
        // mostly because its the simplest way to add it.
        if (toggleInside)
        {
            RenderSettings.fog = !RenderSettings.fog;
            if (RenderSettings.reflectionIntensity != 0)
            {
                reflectionIntensity = RenderSettings.reflectionIntensity;
                RenderSettings.reflectionIntensity = 0;
            }
            else
            {
                RenderSettings.reflectionIntensity = reflectionIntensity;
            }
        }
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