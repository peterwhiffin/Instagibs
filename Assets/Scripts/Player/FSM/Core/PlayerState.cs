using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    public Player _player;
    public PlayerStateMachine _stateMachine;
    public PlayerData _playerData;
    public string _animBoolName;
    public Player.RepData _repData;
    

    public Vector2 _moveInput;
    public Vector2 _lookInput;
    public bool _jumpQueued;
    public bool _shootQueued;
    public bool _jumpPlaying;

    public PlayerState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData)
    {
        this._player = _player;
        this._stateMachine = _stateMachine;
        this._playerData = _playerData;
        this._animBoolName = _animBoolName;
        this._repData = _repData;
    }

    public virtual void Enter() 
    { 
        
    }
    public virtual void Exit()
    {
      
    }
    public virtual void CameraUpdate()
    {       
        _player.RotateCam(_lookInput.y);
    }
    public virtual void TickUpdate(bool asServer) 
    {
        if (!asServer)
        {
            _player.Reconcile(default, false);
            _repData = default; 
            //_repData.shootTimer = _player.ShootTimer(false);
            _repData.lookDir = _lookInput;
            _repData.lookSpeed = _playerData.lookSpeed;
            _repData.gcRay = new Ray(_player._groundCheckPos.position, -_player.transform.up);
            _repData.shoot = _player.InputHandler.ShootQueued;
            _player.InputHandler.ShootQueued = false;
        }
    }
    public virtual void PostTickUpdate()
    {
        if(_player._playerDead && _stateMachine.CurrentState != _player.DeathState)
            _stateMachine.ChangeState(_player.DeathState);

        if (_repData.shoot && !_player._inMenu && _player._shootTimer > .8f)
            _stateMachine.ChangeState(_player.ShootState);

        _player.ShootTimer();
    }
    
    public virtual void ServerRPCMethods() { }
    public virtual void ObserverRPCMethods() { }

    public virtual void OnUpdate()
    {
        _moveInput = _player.InputHandler.MoveInput;
        _lookInput = _player.InputHandler.LookInput;
        _jumpQueued = _player.InputHandler.JumpQueued;
        _shootQueued = _player.InputHandler.ShootQueued;

        if (_jumpPlaying)
        {
            AnimatorStateInfo info = _player._anim.GetCurrentAnimatorStateInfo(0);

            if (info.normalizedTime == .9f)
            {
                _player._anim.Play("Idle");
            }
        }       
    }
}
