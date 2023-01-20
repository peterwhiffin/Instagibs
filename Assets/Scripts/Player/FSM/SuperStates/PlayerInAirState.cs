using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInAirState : PlayerState
{
    public PlayerInAirState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
    {
    }
   
    public override void Enter()
    {
        base.Enter();
        _player._anim.SetFloat("Blend", Mathf.Lerp(_player._anim.GetFloat("Blend"), 0f, .1f));        
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void PostTickUpdate()
    {
        base.PostTickUpdate();

        if (_player._isGrounded)
            _stateMachine.ChangeState(_player.IdleState);

        if (_player._jumpCounter < 1 && _repData.jump)
        {
            _stateMachine.ChangeState(_player.JumpState);
        }
    }

    public override void TickUpdate(bool asServer)
    {
        base.TickUpdate(asServer);

        if (!asServer)
        {            
            _repData.moveDir = _moveInput;
            _repData.jump = _player.InputHandler.JumpQueued;
            _repData.inAir = true;
            _player.Replicate(_repData, false);          
            _player.InputHandler.JumpQueued = false;
        }
    }
}
