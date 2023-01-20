using FishNet.Object;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet;
using FishNet.Broadcast;
using FishNet.Transporting;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public struct PlayerInfo : IBroadcast
    {
        public string Username;
        public int Score;
        public int Slot;
        public int ID;
    }

    public struct ScoreBroadcast : IBroadcast
    {
        public int Score;
        public int ID;
        public bool Reset;
    }

    public struct NewPlayerBroadCast : IBroadcast
    {
        public string Username;
        public int Slot;
        public int ID;
        public Dictionary<int, PlayerInfo> CurrentPlayers;
    }

    public struct RemovePlayerBroadcast : IBroadcast
    {
        public int ID;
    }

    public struct PlayersReadyBroadcast : IBroadcast
    {
        public int PlayersReady;
        public int TotalPlayers;
    }

    public struct StartMatchBroadcast : IBroadcast
    {
        public bool StartMatch;
        public bool MatchStarted;
        public bool MatchFinished;
    }

    public struct EndMatchBroadcast : IBroadcast
    {
        public bool MatchEnded;
        public string Winner;
    }

    public struct TimeBroadcast : IBroadcast
    {
        public string Time;
    }

    public struct CheckMatchStatus : IBroadcast
    {
        public bool StartMatch;
        public bool MatchStarted;
    }

    public struct DespawnBroadcast : IBroadcast
    {

    }

    public int _highestScore;
    public int _playersReady;
    public int _seconds;
    public int _minutes;
    public int _matchLeader;

    public double _matchTime;
    public double _startTime;

    public bool _startMatch;
    public bool _matchStarted;
    public bool _matchFinished;

    public string _winner;


    public Dictionary<int, PlayerInfo> PlayerInfos;
    public Dictionary<int, bool> PlayersReady;


    public override void OnStartServer()
    {
        base.OnStartServer();
        InstanceFinder.ServerManager.RegisterBroadcast<Player.PlayerInfo>(NewPlayer);
        InstanceFinder.ServerManager.RegisterBroadcast<Player.ScoreBroadcast>(UpdateScore);
        InstanceFinder.ServerManager.RegisterBroadcast<ReadyUpUI.ReadyBroadcast>(UpdatePlayersReady);
        InstanceFinder.ServerManager.OnRemoteConnectionState += ConnectionChanged;

        PlayerInfos = new Dictionary<int, PlayerInfo>();
        PlayersReady = new Dictionary<int, bool>();
        _startMatch = false;
        _matchStarted = false;
        _startTime = 10;
        _matchTime = 120;
        _matchFinished = false;
        _highestScore = 0;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        InstanceFinder.ServerManager.UnregisterBroadcast<Player.PlayerInfo>(NewPlayer);
        InstanceFinder.ServerManager.UnregisterBroadcast<Player.ScoreBroadcast>(UpdateScore);
        InstanceFinder.ServerManager.UnregisterBroadcast<ReadyUpUI.ReadyBroadcast>(UpdatePlayersReady);
        InstanceFinder.ServerManager.OnRemoteConnectionState -= ConnectionChanged;
    }

    public void NewPlayer(NetworkConnection conn, Player.PlayerInfo _playerInfo)
    {
        var newPlayer = new PlayerInfo();
        var newPlayerB = new NewPlayerBroadCast();
        newPlayer.ID = newPlayerB.ID = conn.ClientId;
        newPlayer.Username = newPlayerB.Username = _playerInfo.Username;
        newPlayer.Score = 0;
        PlayerInfos.Add(newPlayer.ID, newPlayer);
        PlayersReady.Add(newPlayer.ID, false);
        newPlayerB.CurrentPlayers = PlayerInfos;

        InstanceFinder.ServerManager.Broadcast(newPlayerB);

        var status = new CheckMatchStatus()
        {
            StartMatch = _startMatch,
            MatchStarted = _matchStarted
        };

        InstanceFinder.ServerManager.Broadcast(conn, status);

        if(!_startMatch && !_matchStarted)
        {
            _playersReady = 0;

            foreach (var player in PlayersReady)
            {
                if (player.Value)
                    _playersReady++;
            }

            var readyB = new PlayersReadyBroadcast()
            {
                PlayersReady = _playersReady,
                TotalPlayers = PlayersReady.Count
            };

            InstanceFinder.ServerManager.Broadcast(readyB);
        }
    }

    public void UpdateScore(NetworkConnection conn, Player.ScoreBroadcast score)
    {
        var newInfo = new PlayerInfo();
        newInfo = PlayerInfos[score.ID];
        newInfo.Score++;
        PlayerInfos[score.ID] = newInfo;

        if(newInfo.Score == 2 && _matchStarted)
        {
            _matchStarted = false;
            _matchFinished = true;
        }

        var newScore = new ScoreBroadcast()
        {
            Score = newInfo.Score,
            ID = score.ID,
            Reset = false
        };

        InstanceFinder.ServerManager.Broadcast(newScore);
    }

    public void UpdatePlayersReady(NetworkConnection conn, ReadyUpUI.ReadyBroadcast ready)
    {
        PlayersReady[conn.ClientId] = ready.IsReady;

        _playersReady = 0;
        
        foreach (var player in PlayersReady)
        {
            if(player.Value)
                _playersReady++;
        }

        var readyB = new PlayersReadyBroadcast()
        {
            PlayersReady = _playersReady,
            TotalPlayers = PlayersReady.Count
        };

        InstanceFinder.ServerManager.Broadcast(readyB);

        if(_playersReady == PlayersReady.Count && PlayersReady.Count > 1)
        {
            SetupMatch();
        }
    }

    private void SetupMatch()
    {
        foreach (KeyValuePair<int, bool> player in PlayersReady)
        {
            PlayerInfo info = PlayerInfos[player.Key];
            info.Score = 0;
            PlayerInfos[player.Key] = info;
        }

        var score = new ScoreBroadcast()
        {
            Reset = true
        };

        var status = new CheckMatchStatus()
        {
            StartMatch = true,
            MatchStarted = false
        };

        InstanceFinder.ServerManager.Broadcast(score);
        InstanceFinder.ServerManager.Broadcast(status);

        _startMatch = true;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (_startMatch)
            {
                StartMatch();
            }
            else if (_matchStarted)
            {
                MatchStarted();
            }
            else if (_matchFinished)
            {
                MatchFinished();
            }
            else if (_playersReady == PlayersReady.Count && PlayersReady.Count > 1)
            {
                SetupMatch();
            }
        }
    }

    private void MatchFinished()
    {
        for(int i = 0; i < PlayersReady.Count; i++)
        {
            PlayersReady[i] = false;
        }

        _matchLeader = 0;
        _highestScore = 0;

        foreach(var player in PlayerInfos)
        {
            if(player.Value.Score > _highestScore)
            {
                _highestScore = player.Value.Score;
                _matchLeader = player.Value.ID;
            }
        }

        var end = new EndMatchBroadcast()
        {
            Winner = PlayerInfos[_matchLeader].Username
        };

        var ready = new PlayersReadyBroadcast()
        {
            PlayersReady = 0,
            TotalPlayers = PlayersReady.Count
        };

        InstanceFinder.ServerManager.Broadcast(end);
        InstanceFinder.ServerManager.Broadcast(ready);

        _startTime = 10;
        _matchTime = 120;
        _matchFinished = false;
    }

    private void MatchStarted()
    {
        var floatedTime = (float)_matchTime;

        if (floatedTime < 0)
            floatedTime = 0;

        var time = new TimeBroadcast()
        {
            Time = string.Format("{0:0}:{1:00}", Mathf.FloorToInt(floatedTime / 60), Mathf.FloorToInt(floatedTime % 60))
        };

        InstanceFinder.ServerManager.Broadcast(time);

        _matchTime -= base.TimeManager.TickDelta;

        if (floatedTime <= 0f)
        {
            var start = new CheckMatchStatus()
            {
                StartMatch = false,
                MatchStarted = false
            };

            InstanceFinder.ServerManager.Broadcast(start);

            _matchStarted = false;
            _matchFinished = true;
        }
    }

    private void StartMatch()
    {
        var floatedTime = (float)_startTime;


        var time = new TimeBroadcast()
        {
            Time = string.Format("{0:0}", Mathf.FloorToInt(floatedTime % 60))
        };

        InstanceFinder.ServerManager.Broadcast(time);

        _startTime -= base.TimeManager.TickDelta;

        if (floatedTime <= 0f)
        {
            var start = new CheckMatchStatus()
            {
                StartMatch = false,
                MatchStarted = true
            };

            InstanceFinder.ServerManager.Broadcast(start);

            _startMatch = false;
            _matchStarted = true;          
        }
    }

    public void ConnectionChanged(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if(args.ConnectionState == RemoteConnectionState.Stopped && IsServer)
        {
            Debug.Log("RUNNING CONN CHANGED");
            //PlayerDisconnect(conn);

            Debug.Log("Player ID disconnected: " + conn.ClientId);

            PlayerInfos.Remove(conn.ClientId);
            PlayersReady.Remove(conn.ClientId);

            InstanceFinder.ServerManager.Despawn(conn.FirstObject);

            var player = new RemovePlayerBroadcast()
            {
                ID = conn.ClientId
            };

            InstanceFinder.ServerManager.Broadcast(player);

            _playersReady = 0;

            foreach (var playerReady in PlayersReady)
            {
                if (playerReady.Value)
                    _playersReady++;
            }

            var readyB = new PlayersReadyBroadcast()
            {
                PlayersReady = _playersReady,
                TotalPlayers = PlayersReady.Count
            };

            Debug.Log(PlayersReady.Count);

            InstanceFinder.ServerManager.Broadcast(readyB);
        }
    }

    public void PlayerDisconnect(NetworkConnection conn)
    {
        Debug.Log("Player ID disconnected: " + conn.ClientId);

        PlayerInfos.Remove(conn.ClientId);
        PlayersReady.Remove(conn.ClientId);
        
        InstanceFinder.ServerManager.Despawn(conn.FirstObject);

        var player = new RemovePlayerBroadcast()
        {
            ID = conn.ClientId
        };

        InstanceFinder.ServerManager.Broadcast(player);

        _playersReady = 0;

        foreach (var playerReady in PlayersReady)
        {
            if (playerReady.Value)
                _playersReady++;
        }

        var readyB = new PlayersReadyBroadcast()
        {
            PlayersReady = _playersReady,
            TotalPlayers = PlayersReady.Count
        };

        Debug.Log(PlayersReady.Count);

        InstanceFinder.ServerManager.Broadcast(readyB);
    }
}
