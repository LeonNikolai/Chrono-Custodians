using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class MovementModifierManager
{
    private Dictionary<MovementModifier, float> movementModifierValues = new Dictionary<MovementModifier, float>()
    {
        { MovementModifier.None, 1f },
        { MovementModifier.Sprinting, 1.75f },
        { MovementModifier.WeatherStorm, 0.5f }
    };

    public float movementMultiplier;
    private MovementModifier activeModifiers = MovementModifier.None;
    
    public void ActivateModifier(MovementModifier modifier)
    {
        activeModifiers |= modifier;
        CalculateMovementMultiplier();
    }

    public void DeactivateModifier(MovementModifier modifier)
    {
        activeModifiers &= ~modifier;
        CalculateMovementMultiplier();
    }

    private void CalculateMovementMultiplier()
    {
        movementMultiplier = 1f;
        
        foreach (MovementModifier modifier in Enum.GetValues(typeof(MovementModifier)))
        {
            if (activeModifiers.HasFlag(modifier))
            {
                movementMultiplier *= movementModifierValues[modifier];
            }
        }
    }

    public void ResetModifiers()
    {
        activeModifiers = MovementModifier.None;
        CalculateMovementMultiplier();
    }
}