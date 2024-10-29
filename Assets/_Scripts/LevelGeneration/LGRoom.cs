using System.Text;
using Unity.Netcode;
using UnityEngine;

public class LGRoom : MonoBehaviour
{
    [SerializeField] private Vector3 roomSize;
    [SerializeField] private Vector3 transformOffset;
    [SerializeField] public GameObject ignoreCheck;
    [SerializeField] private LGEntryPointController[] entryPoints;

    /*
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
    }*/

    public LGEntryPointController AlignRoomToEntryPoint(LGEntryPointController targetEntryPoint)
    {
        LGEntryPointController entryPoint = entryPoints[Random.Range(0, entryPoints.Length)];
        // Calculate the rotation needed to align the entry point with the target entry point
        Quaternion rotationToMatch = Quaternion.LookRotation(-targetEntryPoint.transform.forward, targetEntryPoint.transform.up);
        Quaternion entryRotation = Quaternion.LookRotation(entryPoint.transform.forward, entryPoint.transform.up);
        Quaternion finalRotation = rotationToMatch * Quaternion.Inverse(entryRotation);

        transform.rotation = finalRotation * transform.rotation;

        // Calculate the position offset needed to match entry point to target entry point
        Vector3 positionOffset = targetEntryPoint.transform.position - entryPoint.transform.position;
        transform.position += positionOffset;

        // Check if the room is colliding after applying the position and rotation
        if (!isColliding())
        {
            Debug.Log("Room aligned and placed successfully.");
            return null;
        }
        return entryPoint;
    }


    public bool isColliding()
    {
        ignoreCheck.SetActive(false);
        Collider[] hitColliders = Physics.OverlapBox(transform.position + transformOffset, roomSize / 2, Quaternion.identity);
        if (hitColliders.Length <= 0)
        {
            ignoreCheck.SetActive(true);
            AddEntryPoints();
            return true;
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
