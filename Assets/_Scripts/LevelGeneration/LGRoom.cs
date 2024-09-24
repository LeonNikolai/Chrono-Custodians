using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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
            // Remove entrypoint from room
            entryPoint.transform.SetParent(null);
            
            // Set this room to be a child of the entry point
            transform.SetParent(entryPoint.transform);

            // Set entrypoint's rotation and position to be equal to the target Entrypoint
            entryPoint.transform.rotation = Quaternion.Inverse(targetEntryPoint.transform.rotation);
            entryPoint.transform.position = targetEntryPoint.transform.position;

            // Release room from entrypoint
            transform.SetParent(null);

            // put entry point back on room
            entryPoint.transform.SetParent(ignoreCheck.transform);
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
