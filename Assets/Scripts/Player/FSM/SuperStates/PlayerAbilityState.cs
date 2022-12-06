using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilityState : PlayerState
{
    

    public bool _abilityDone;

    public PlayerAbilityState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
    {
    }

    public override void CameraUpdate()
    {
        base.CameraUpdate();
    }

    public override void Enter()
    {
        base.Enter();
        _abilityDone = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PostTickUpdate()
    {
        base.PostTickUpdate();
    }

    public override void TickUpdate(bool asServer)
    {
        base.TickUpdate(asServer);
    }
}
