using System;
using System.Collections.Generic;
using System.Numerics;

public class MovementStateManager
{
    public Dictionary<MovementState, MovementData> movementStateValues = new Dictionary<MovementState, MovementData>()
    {
        { MovementState.Walking, new MovementData(1f, new Vector3(1f, 0f, 1f)) },
        { MovementState.Crouching, new MovementData(0.3f, new Vector3(1, 0f, 1f)) },
        { MovementState.Climbing, new MovementData(0.5f, new Vector3(0f, 1f, 0f)) },
        { MovementState.Jetpack, new MovementData(0.75f, new Vector3(1f, 0f, 1f)) }
    };
    
    public MovementData data;
    public MovementState currentState = MovementState.Walking;

    public void GetMovementData(MovementState state)
    {
        if (movementStateValues.TryGetValue(state, out MovementData data))
        {
            this.data = data;
            currentState = state;
        }
    }
}