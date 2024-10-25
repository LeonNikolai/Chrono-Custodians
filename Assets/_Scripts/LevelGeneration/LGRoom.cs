using System.Text;
using Unity.Netcode;
using UnityEngine;

public class LGRoom : MonoBehaviour
{
    [SerializeField] private Vector3 roomSize;
    [SerializeField] private Vector3 transformOffset;
    [SerializeField] public GameObject ignoreCheck;
    [SerializeField] private LGEntryPointController[] entryPoints;

    public void AlignRoomToEntryPoint(LGEntryPointController targetEntryPoint)
    {
        foreach (var entryPoint in entryPoints)
        {
            NetworkObject entry = entryPoint.GetComponent<NetworkObject>();
            NetworkObject self = GetComponent<NetworkObject>();
            if (!entry.IsSpawned)
            {
                entry.Spawn();
            }
            // Remove entrypoint from room
            if (entry.TryRemoveParent())
            {
                self.TrySetParent(entryPoint.transform);
            }

            // Set entrypoint's rotation and position to be equal to the target Entrypoint
            entryPoint.transform.rotation = new Quaternion(0, 0, 0, 0);
            entryPoint.transform.position = targetEntryPoint.transform.position;

            // Release room from entrypoint
            self.TryRemoveParent();

            // put entry point back on room
            entry.TrySetParent(ignoreCheck.transform);
            targetEntryPoint.DisableRoomCollision(true);
            if (!isColliding())
            {
                Debug.Log("Room is colliding");
                break;
            }
            else
            {
                targetEntryPoint.DisableRoomCollision(false);
                Debug.Log("Room is not colliding. Placing..");
            }
        }
    }

    public bool isColliding()
    {
        ignoreCheck.SetActive(false);
        Collider[] hitColliders = Physics.OverlapBox(transform.position + transformOffset, roomSize / 2, Quaternion.identity);
        if (hitColliders.Length <= 0)
        {
            ignoreCheck.SetActive(true);
            AddEntryPoints();
            return false;
        }
        ignoreCheck.SetActive(true);
        return true;
    }

    public void AddEntryPoints()
    {
        foreach (LGEntryPointController entrypoint in entryPoints)
        {
            entrypoint.CheckNear();
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + transformOffset, roomSize);
    }
}
