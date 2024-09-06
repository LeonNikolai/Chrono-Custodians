using UnityEngine;

public class Ladder : MonoBehaviour, IHighlightable, IInteractable
{
    [SerializeField] private Collider[] endPoint;
    
    private void OnTriggerEnter(Collider hit)
    {
        if (hit.gameObject.CompareTag("Player")) 
        {
            hit.gameObject.GetComponent<PlayerMovement>().ChangeState(MovementState.Climbing, 0f);
            Debug.Log("a");
        }
    }
    
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
    
    public void Interact() 
    {
        Debug.Log("Interact");
    }
}
