using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShootState : PlayerAbilityState
{
    public PlayerShootState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
    {
    }

    public override void Enter()
    {
        base.Enter();

    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PostTickUpdate()
    {
        base.PostTickUpdate();

        _stateMachine.ChangeState(_player.IdleState);
    }

    public override void TickUpdate(bool asServer)
    {
        base.TickUpdate(asServer);

        //if (!asServer)
        //{
        //    _repData.moveDir = _moveInput;
        //    _repData.doShoot = true;
        //    _repData.shootRay = _player._cam.ScreenPointToRay(_player._crosshair.transform.position);
        //    _player.Replicate(_repData, false);
        //}

        _player.Shoot(_player.Owner, _player._cam.ScreenPointToRay(_player._crosshair.transform.position));
    }
}
