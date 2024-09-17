using UnityEngine;

public class Ladder : MonoBehaviour, IHighlightable, IInteractable
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float rotation;
    [SerializeField] private bool endPoint, rotateAtSpawn;

    public void HightlightEnter() 
    {
        Debug.Log("HighLight Enter");
    }
    
    public void HightlightUpdate() 
    {
        
    }
    
    public void HightlightExit() 
    {
        Debug.Log("HighLight Exit");
    }
    
    public void Interact(PlayerMovement player) 
    {
        if (player.movementState.currentState != MovementState.Climbing) 
        {
            player.ChangeState(MovementState.Climbing, 0f);
            player.ChangePosition(spawnPoint.position);
            SetPlayerRotation(player);
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
