using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine
{
    public PlayerState CurrentState;
    public PlayerState NextState;

    public void Initialize(PlayerState _startingState)
    {
        CurrentState = _startingState;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState _newState)
    {
        NextState = _newState;
        CurrentState.Exit();
        CurrentState = _newState;
        CurrentState.Enter();
    }
}
