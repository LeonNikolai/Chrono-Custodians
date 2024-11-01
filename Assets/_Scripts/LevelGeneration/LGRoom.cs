using System.Text;
using Unity.Netcode;
using UnityEngine;

public enum RoomType
{
    Corridor,
    Room
}

public class LGRoom : MonoBehaviour
{
    [SerializeField] private Vector3 roomSize;
    [SerializeField] private Vector3 transformOffset;
    [SerializeField] public GameObject ignoreCheck;
    [SerializeField] private LGEntryPointController[] entryPoints;
    [SerializeField] public RoomType roomType;

    private Bounds roomBounds;


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
        //ignoreCheck?.SetActive(false);
        Collider[] hitColliders = Physics.OverlapBox(transform.position + transformOffset, roomSize / 2, Quaternion.identity);
        if (hitColliders.Length <= 0)
        {
            //ignoreCheck?.SetActive(true);
            //AddEntryPoints();
            return true;
        }
        //ignoreCheck?.SetActive(true);
        return true;
    }

    public void AddEntryPoints()
    {
        foreach (LGEntryPointController entrypoint in entryPoints)
        {
            entrypoint.CheckNear();
        }
    }

    public void SetRoomBounds()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            // Set roomSize based on the size of the MeshRenderer's bounds
            roomSize = meshRenderer.bounds.size;

            // Set transformOffset based on the center offset of the bounds
            transformOffset = meshRenderer.bounds.center - transform.position;
        }
        else
        {
            Debug.LogWarning("No MeshRenderer found on the room. Please add one or set bounds manually.");
        }
    }

    public Bounds GetOrientedBounds()
    {
        Vector3[] corners = new Vector3[8];
        Vector3 halfSize = roomSize / 2;

        // Define the 8 corners of the box in local space
        corners[0] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        corners[1] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        corners[2] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        corners[3] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        corners[4] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        corners[5] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        corners[6] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        corners[7] = new Vector3(halfSize.x, halfSize.y, halfSize.z);

        // Transform corners to world space
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = transform.TransformPoint(corners[i] + transformOffset);
        }

        // Calculate the axis-aligned bounds of the rotated box
        Vector3 min = corners[0];
        Vector3 max = corners[0];
        foreach (Vector3 corner in corners)
        {
            min = Vector3.Min(min, corner);
            max = Vector3.Max(max, corner);
        }

        return new Bounds((min + max) / 2, max - min);
    }

    public bool IsOverlapping(LGRoom otherRoom, float overlapThreshold)
    {
        Bounds thisBounds = GetOrientedBounds();
        Bounds otherBounds = otherRoom.GetOrientedBounds();

        if (thisBounds.Intersects(otherBounds))
        {
            Bounds intersection = GetIntersectionBounds(thisBounds, otherBounds);
            float overlapVolume = intersection.size.x * intersection.size.y * intersection.size.z;
            return overlapVolume > overlapThreshold;
        }
        return false;
    }

    private Bounds GetIntersectionBounds(Bounds a, Bounds b)
    {
        Vector3 min = Vector3.Max(a.min, b.min);
        Vector3 max = Vector3.Min(a.max, b.max);
        return new Bounds((min + max) / 2, max - min);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        // Set the matrix to match the transform's position, rotation, and scale
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

        // Draw the wire cube with rotation applied
        Gizmos.DrawWireCube(transformOffset, roomSize);

        // Reset the matrix to avoid affecting other gizmos
        Gizmos.matrix = Matrix4x4.identity;
    }

}
