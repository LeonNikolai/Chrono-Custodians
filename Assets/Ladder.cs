using UnityEngine;

public class Ladder : MonoBehaviour, IHighlightable, IInteractable
{
    [SerializeField] private Vector3 startClimbPoint;
    
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
            player.transform.position = startClimbPoint;
        }
    }
}
