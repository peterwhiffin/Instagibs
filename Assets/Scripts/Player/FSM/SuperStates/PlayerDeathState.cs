
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeathState : PlayerState
{
    
    public PlayerDeathState(Player _player, PlayerStateMachine _stateMachine, PlayerData _playerData, string _animBoolName, Player.RepData _repData) : base(_player, _stateMachine, _playerData, _animBoolName, _repData)
    {
    }

    Transform newSpawn;

    public override void CameraUpdate()
    {
        base.CameraUpdate();
        _player._camObject.transform.localPosition = Vector3.Lerp(_player._camObject.transform.localPosition, _player._deathCamPos.localPosition, .08f);
        _player._camObject.transform.LookAt(_player._playerModel.transform);
    }

    public override void Enter()
    {
        base.Enter();
        _player.RespawnTimer(true);
        newSpawn = _player._spawns[Random.Range(0, _player._spawns.Length)];
    }

    public override void Exit()
    {
        base.Exit();

        //_player._camObject.transform.parent = _player._camParent;
        _player._camObject.transform.localPosition = _player._cameDefaultPos;
        _player._camObject.transform.localEulerAngles = Vector3.zero;
    }

    public override void PostTickUpdate()
    {
        base.PostTickUpdate();

        if (!_player._playerDead)
            _stateMachine.ChangeState(_player.IdleState);
    }

    public override void TickUpdate(bool asServer)
    {
        base.TickUpdate(asServer);

        if (!asServer)
        {
            _player.RespawnTimer(false);
            _repData.respawn = false;
            _repData = default;
            _repData.dead = _player._playerDead;
            _repData.respawnTimer = _player._respawnTimer;
            _repData.spawnPos = newSpawn.position;
            _repData.spawnRot = newSpawn.eulerAngles;
            _player.Replicate(_repData, false);
        }
    }
}
