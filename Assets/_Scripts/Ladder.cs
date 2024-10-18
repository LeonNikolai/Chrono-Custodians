using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float rotation;
    [SerializeField] private bool endPoint, rotateAtSpawn;

    public bool Interactable => true;

    public void Interact(Player player) 
    {
        var movment = player.Movement;
        if (movment.movementState.currentState != MovementState.Climbing) 
        {
            movment.ChangeState(MovementState.Climbing, 0f);
            movment.ChangePosition(spawnPoint.position);
            SetPlayerRotation(movment);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (endPoint && other.gameObject.CompareTag("Player")) 
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            
            if (player.movementState.currentState == MovementState.Climbing) 
            {
                player.ChangeState(MovementState.Walking, 1f);
                player.ChangePosition(spawnPoint.position);
                
                if (rotateAtSpawn) 
                {
                    SetPlayerRotation(player);
                }
            }
        }
    }

    private void SetPlayerRotation(PlayerMovement player)
    {
        Quaternion ladderRotation = transform.parent.transform.rotation;
        Vector3 newRotation = new(0f, ladderRotation.eulerAngles.y, 0f);
        player.xRotation = rotation;
        player.transform.rotation = Quaternion.Euler(newRotation);
    }
}
