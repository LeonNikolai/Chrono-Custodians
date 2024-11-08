using Unity.Netcode;
using UnityEngine;

public class LGEntryPointController : MonoBehaviour
{
    [SerializeField] private LayerMask entryPoint;
    [SerializeField] private LGRoom ownerRoom;

    void Awake()
    {

    }

    void Start()
    {
        bool isClient = NetworkManager.Singleton.IsClient;
        bool isHost = NetworkManager.Singleton.IsHost;
        if (isClient && !isHost)
        {
            Destroy(gameObject);
        }
    }

    public RoomType GetRoomType()
    {
        return ownerRoom.roomType;
    }

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

    public void fillPoint()
    {

    }

}
