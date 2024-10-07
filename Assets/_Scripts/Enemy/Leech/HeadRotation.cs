using UnityEngine;
using System.Collections;
using System.Net.Sockets;

public class HeadRotation : MonoBehaviour
{

    // This should be Networkbehavior so it's synced with all clients.


    [SerializeField] private FieldOfView fov;

    [SerializeField] private Vector3 rotationLimit; // Rotation limit for x, y, and z axis
    [SerializeField] private float rotationDuration = 5f; // How long the random rotations last
    [SerializeField] private float rotationInterval = 10f; // The time between random rotations
    [SerializeField] private float switchInterval = 0.07f; // How often to switch to a new random rotation
    [SerializeField] private float headturnSpeed = 2f; // Head rotation speed

    private Quaternion returnRotation;
    private Quaternion originalRotation;
    private bool isRotating = false;

    private Transform target;

    void Start()
    {
        originalRotation = transform.localRotation;
        returnRotation = transform.localRotation;
        // Start the glitchy rotation routine at intervals
        InvokeRepeating(nameof(TriggerGlitchyRotation), rotationInterval, rotationInterval);
    }

    void TriggerGlitchyRotation()
    {
        if (!isRotating)
        {
            StartCoroutine(PerformGlitchyRandomRotation());
        }
    }

    IEnumerator PerformGlitchyRandomRotation()
    {

        isRotating = true;
        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            // Instantly set to a new random rotation within the limits
            Quaternion randomRotation = Quaternion.Euler(
                Random.Range(-rotationLimit.x, rotationLimit.x),
                Random.Range(-rotationLimit.y, rotationLimit.y),
                Random.Range(-rotationLimit.z, rotationLimit.z)
            );

            // Instantly apply the random rotation (glitchy effect)
            transform.rotation = randomRotation;

            // Wait for the specified switch interval before glitching again
            yield return new WaitForSeconds(switchInterval);

            // Track the time spent rotating
            elapsedTime += switchInterval;
        }

        // After the glitchy rotation phase, return to the original rotation
        transform.localRotation = returnRotation;
        isRotating = false;
    }

    private void Update()
    {
        if (target != null)
        {
            if (!isRotating)
            {
                Vector3 targetPosition = new Vector3(target.position.x, target.position.y + 1f, target.position.z);
                Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);
                returnRotation = targetRotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, returnRotation, Time.deltaTime * headturnSpeed);
            }
        }

        if (fov == null) return;
        if (fov.canSeeTarget && target == null)
        {
            if (fov.curtarget)
            {
                LookTarget(fov.curtarget.transform);
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
