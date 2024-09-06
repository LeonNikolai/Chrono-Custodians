using UnityEngine;

public class StateObject : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Player") 
        {
            other.gameObject.GetComponent<PlayerMovement>().ChangeState(MovementState.Climbing);
        }
    }
}
