using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerGroundedState
{
    public PlayerMoveState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
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

        if (_repData.jump && _player._isGrounded)
            _stateMachine.ChangeState(_player.JumpState);

        if (_repData.moveDir == Vector2.zero)
            _stateMachine.ChangeState(_player.IdleState);
    }

    public override void TickUpdate(bool asServer)
    {
        base.TickUpdate(asServer);

        if (!asServer)
        {
            _repData.jump = _player.InputHandler.JumpQueued;
            _repData.moveDir = _moveInput;
            _player.InputHandler.JumpQueued = false;
            _player.Replicate(_repData, false);
        }
    }
}
