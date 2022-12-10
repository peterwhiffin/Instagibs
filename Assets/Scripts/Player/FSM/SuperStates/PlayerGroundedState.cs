using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    public PlayerGroundedState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _player.JumpCounter(true);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PostTickUpdate()
    {
        base.PostTickUpdate();

        if (!_player._isGrounded)
            _stateMachine.ChangeState(_player.InAirState);
    }

    public override void TickUpdate(bool asServer)
    {
        base.TickUpdate(asServer);
    }

    public override void OnUpdate()
    {
        base.OnUpdate();

        _player._anim.SetFloat("Blend", Mathf.Lerp(_player._anim.GetFloat("Blend"), Mathf.Abs(_moveInput.x) + Mathf.Abs(_moveInput.y), .1f));
    }
}
