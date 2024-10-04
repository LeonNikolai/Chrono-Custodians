using UnityEngine;

public class LGEntryPointController : MonoBehaviour
{
    [SerializeField] private LayerMask entryPoint;
    [SerializeField] private LGRoom ownerRoom;
    public void CheckNear()
    {
        Collider[] hit = Physics.OverlapSphere(transform.position, 0.5f, entryPoint); 
        if (hit.Length > 0)
        {
            Destroy(gameObject);
        }
        else
        {
            LGManager.Instance.AddEntryPoint(this);
        }
    }

    public void DisableRoomCollision(bool disable)
    {
        ownerRoom.ignoreCheck.SetActive(disable);
    }

}
