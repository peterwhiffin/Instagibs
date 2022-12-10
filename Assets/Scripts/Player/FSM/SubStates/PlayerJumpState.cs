using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerAbilityState
{
    public PlayerJumpState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
    {
    }

    public override void CameraUpdate()
    {
        base.CameraUpdate();
    }

    public override void Enter()
    {
        base.Enter();

        _player._networkAnim.Play("Jump", 0, 0f);
        _jumpPlaying = true;
        _player.JumpCounter(false);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PostTickUpdate()
    {
        base.PostTickUpdate();

        _stateMachine.ChangeState(_player.InAirState);
    }

    public override void TickUpdate(bool asServer)
    {
        base.TickUpdate(asServer);

        if (!asServer)
        {
            _repData.doJump = true;
            _repData.moveDir = _moveInput;
            _player.Replicate(_repData, false);
        }
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
    }
}
