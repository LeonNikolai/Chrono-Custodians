using System.Numerics;

public struct MovementData
{
    public float speed;
    public Vector3 direction;

    public MovementData(float speed, Vector3 direction)
    {
        this.speed = speed;
        this.direction = direction;
    }
}

public enum MovementState
{
    Walking,
    Crouching,
    Climbing
}