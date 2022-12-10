using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootState : PlayerAbilityState
{
    public PlayerShootState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
    {
    }

    private bool _hasFired;

    public override void Enter()
    {
        base.Enter();
        _hasFired = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PostTickUpdate()
    {
        base.PostTickUpdate();

        if(_player._shootTimer < .8d)
            _stateMachine.ChangeState(_player.IdleState);
    }

    public override void TickUpdate(bool asServer)
    {        
        base.TickUpdate(asServer);

        if (!_hasFired)
        {
            _player.Shoot(_player._cam.ScreenPointToRay(_player._crosshair.transform.position));
            _hasFired = true;
        }

        if (!asServer)
        {
            _repData.moveDir = _moveInput;
            _player.Replicate(_repData, false);
        }
    }
}
