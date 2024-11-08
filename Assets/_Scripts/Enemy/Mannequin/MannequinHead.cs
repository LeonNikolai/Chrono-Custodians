using UnityEngine;

public class MannequinHead : MonoBehaviour
{

    // This should be Networkbehavior so it's synced with all clients.


    [SerializeField] private FieldOfView fov;

    [SerializeField] private Vector3 rotationLimit; // Rotation limit for x, y, and z axis
    [SerializeField] private float headturnSpeed = 2f; // Head rotation speed

    private Quaternion returnRotation;
    private Quaternion originalRotation;
    private bool startRotating = false;

    private Transform target;

    void Start()
    {
        originalRotation = transform.localRotation;
        returnRotation = transform.localRotation;
    }

    public void StartRotating(bool _startRotating)
    {
        startRotating = _startRotating;
    }

    public void SetHeadToTarget()
    {
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, target.position.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);

        // Apply a correction to align the Z-axis properly
        Quaternion correction = Quaternion.Euler(0, 0, -90); // Adjust this to fix the tilt (e.g., 90 degrees on the Z-axis)
        transform.rotation = targetRotation * correction;
    }

    private void Update()
    {
        if (target != null)
        {
            if (startRotating)
            {
                Vector3 targetPosition = new Vector3(target.position.x, target.position.y, target.position.z);
                Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position, Vector3.up);

                // Apply a correction to align the Z-axis properly
                Quaternion correction = Quaternion.Euler(0, 0, -90); // Adjust this to fix the tilt (e.g., 90 degrees on the Z-axis)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * correction, Time.deltaTime * headturnSpeed);

            }
        }

        if (fov == null) return;
        if (fov.canSeeTarget && target == null)
        {
            if (fov.curtarget)
            {
                
                LookTarget(fov.curtarget.GetComponent<Player>().Camera.transform);
            }
        }else if (!fov.canSeeTarget && target != null)
        {
            target = null;
            returnRotation = originalRotation;
        }
    }

    public void LookTarget(Transform targetTransform)
    {
        target = targetTransform;
    }
}
