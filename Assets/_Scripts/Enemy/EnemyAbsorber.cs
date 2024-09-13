using Unity.Services.Matchmaker.Models;
using UnityEngine;

enum AbsorberState
{
    Roaming,
    Discovery, // The Absorber has discovered a player. In this behavior it will stop and observe the player.

}

public class EnemyAbsorber : Enemy
{
    [Header("Absorber Specific")]
    [SerializeField] private AbsorberState state;

    private void SwitchState(AbsorberState state)
    {
        isRoaming = false;
        switch(state)
        {
            case AbsorberState.Roaming:
                StartRoaming();
                break;

            case AbsorberState.Discovery:
                StopRoaming();
                break;
        }
    }
    
    public override void Attack()
    {
        throw new System.NotImplementedException();
    }
}
