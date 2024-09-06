using UnityEngine;

public class LadderStopPoint : MonoBehaviour
{
    [SerializeField] private Vector3 spawnPoint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            
            if (player.movementState.currentState == MovementState.Climbing) 
            {
                player.ChangeState(MovementState.Walking, 1f);
                player.transform.position = spawnPoint;
            }
        }
    }
}
