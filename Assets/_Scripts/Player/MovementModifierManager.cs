using System;
using System.Collections.Generic;

public partial class MovementModifierManager
{
    private Dictionary<MovementModifier, float> movementModifierValues = new Dictionary<MovementModifier, float>()
    {
        { MovementModifier.Sprinting, 1.75f },
        { MovementModifier.WeatherStorm, 0.5f }
    };

    private MovementModifier activeModifiers = MovementModifier.None;

    public void ActivateModifier(MovementModifier modifier)
    {
        activeModifiers |= modifier;
    }

    public void DeactivateModifier(MovementModifier modifier)
    {
        activeModifiers &= ~modifier;
    }

    public float CalculateMovementMultiplier()
    {
        float movementMultiplier = 1.0f;

        foreach (MovementModifier modifier in Enum.GetValues(typeof(MovementModifier)))
        {
            if (activeModifiers.HasFlag(modifier) && modifier != MovementModifier.None)
            {
                movementMultiplier *= movementModifierValues[modifier];
            }
        }
        return movementMultiplier;
    }

    public void ResetModifiers()
    {
        activeModifiers = MovementModifier.None;
    }
}
