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

    public double _matchTime;

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
        //InstanceFinder.ServerManager.RegisterBroadcast<ReadyUpUI.ReadyBroadcast>(PlayerReady);
        InstanceFinder.ServerManager.OnRemoteConnectionState += ConnectionChanged;

        PlayerInfos = new Dictionary<int, PlayerInfo>();
        //PlayersReady = new Dictionary<int, bool>();
        //_matchStarted = false;
        //_matchTime = 10;
        //_matchFinished = false;
        //_seconds = 10;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        InstanceFinder.ServerManager.UnregisterBroadcast<Player.PlayerInfo>(NewPlayer);
        InstanceFinder.ServerManager.UnregisterBroadcast<Player.ScoreBroadcast>(UpdateScore);
        //InstanceFinder.ServerManager.UnregisterBroadcast<ReadyUpUI.ReadyBroadcast>(PlayerReady);
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
        newPlayerB.CurrentPlayers = PlayerInfos;

        InstanceFinder.ServerManager.Broadcast(newPlayerB);
    }

    public void UpdateScore(NetworkConnection conn, Player.ScoreBroadcast score)
    {
        var newInfo = new PlayerInfo();
        newInfo = PlayerInfos[score.ID];
        newInfo.Score++;
        PlayerInfos[score.ID] = newInfo;

        var newScore = new ScoreBroadcast()
        {
            Score = newInfo.Score,
            ID = score.ID
        };

        InstanceFinder.ServerManager.Broadcast(newScore);
    }


    //private void Update()
    //{
    //    if (IsServer)
    //    {
    //        if (_startMatch)
    //        {
    //            foreach (KeyValuePair<int, bool> player in PlayersReady)
    //            {
    //                PlayerInfo info = PlayerInfos[player.Key];
    //                info.Score = 0;
    //                PlayerInfos[player.Key] = info;
    //            }

    //            var newB = new ScoreBroadcast()
    //            {
    //                Reset = true
    //            };

    //            var end = new EndMatchBroadcast()
    //            {
    //                MatchEnded = false
    //            };

    //            InstanceFinder.ServerManager.Broadcast(end);
    //            InstanceFinder.ServerManager.Broadcast(newB);

    //            float floatedTime = ((float)_matchTime);

    //            var seconds = Mathf.FloorToInt(floatedTime % 60);

    //            if (_seconds != seconds)
    //            {
    //                var time = new TimeBroadcast()
    //                {
    //                    Time = seconds.ToString()
    //                };

    //                InstanceFinder.ServerManager.Broadcast(time);
    //                _seconds = seconds;
    //            }

    //            _matchTime -= base.TimeManager.TickDelta;

    //            if (seconds == 0)
    //            {
    //                var start = new StartMatchBroadcast()
    //                {
    //                    StartMatch = false,
    //                    MatchStarted = true,
    //                    MatchFinished = false
    //                };

    //                InstanceFinder.ServerManager.Broadcast(start);
    //                _matchFinished = false;
    //                _matchStarted = true;
    //                _startMatch = false;
    //                _matchTime = 150;
    //                _seconds = 10;
    //            }
    //        }

    //        if (_matchStarted)
    //        {
    //            _startMatch = false;
    //            float floatedTime = ((float)_matchTime);

    //            var minutes = Mathf.FloorToInt(floatedTime / 60);
    //            var seconds = Mathf.FloorToInt(floatedTime % 60);

    //            if (_seconds != seconds || _minutes != minutes)
    //            {
    //                var time = new TimeBroadcast()
    //                {
    //                    Time = string.Format("{0:0}:{1:00}", minutes, seconds)
    //                };

    //                InstanceFinder.ServerManager.Broadcast(time);
    //                _seconds = seconds;
    //                _minutes = minutes;
    //            }

    //            if (minutes == 0 && seconds == 0)
    //            {
    //                _matchFinished = true;
    //            }

    //            _matchTime -= base.TimeManager.TickDelta;
    //        }

    //        if (_matchFinished && _matchStarted)
    //        {
    //            _matchFinished = false;
    //            _matchStarted = false;
    //            _matchTime = 10;

    //            _highestScore = 0;
    //            foreach(KeyValuePair<int, PlayerInfo> player in PlayerInfos)
    //            {
    //                PlayersReady[player.Key] = false;

    //                if (player.Value.Score > _highestScore)
    //                {
    //                    _highestScore = player.Value.Score;
    //                    _winner = player.Value.Username;
    //                }                   
    //            }

    //            var end = new EndMatchBroadcast()
    //            {
    //                MatchEnded = true,
    //                Winner = _winner,
    //            };

    //            var ready = new PlayersReadyBroadcast()
    //            {
    //                PlayersReady = 0,
    //                TotalPlayers = PlayersReady.Count
    //            };

    //            InstanceFinder.ServerManager.Broadcast(end);
    //            InstanceFinder.ServerManager.Broadcast(ready);
    //        }
    //    }
    //}

    //public void NewPlayer(NetworkConnection conn, Player.PlayerInfo info)
    //{
    //    var newInfo = new PlayerInfo();
    //    var broadcastInfo = new NewPlayerBroadCast();
    //    newInfo.Username = broadcastInfo.Username = info.Username;
    //    newInfo.Score = 0;
    //    newInfo.Slot = broadcastInfo.Slot = PlayerInfos.Count;
    //    newInfo.ID = broadcastInfo.ID = conn.ClientId;
    //    broadcastInfo.Conn = conn;
    //    broadcastInfo.CurrentPlayers = PlayerInfos;
    //    PlayerInfos.Add(conn.ClientId, newInfo);
    //    PlayersReady.Add(conn.ClientId, false);

    //    InstanceFinder.ServerManager.Broadcast(broadcastInfo);

    //    var matchStatus = new CheckMatchStatus()
    //    {
    //        MatchStarted = _matchStarted,
    //        StartMatch = _startMatch
    //    };

    //    InstanceFinder.ServerManager.Broadcast(matchStatus);
    //}

    //public void UpdateScore(NetworkConnection conn, Player.ScoreBroadcast SB)
    //{
    //    PlayerInfo info = PlayerInfos[SB.ID];
    //    info.Score++;
    //    PlayerInfos[SB.ID] = info;

    //    var newB = new ScoreBroadcast()
    //    {
    //        Score = info.Score,
    //        Slot = info.Slot,
    //        Reset = false
    //    };

    //    InstanceFinder.ServerManager.Broadcast(newB);

    //    if (info.Score == 10)
    //        _matchFinished = true;
    //}

    //public void PlayerDisconnect(NetworkConnection conn, Player.DisconnectBroadcast info)
    //{
    //    var player = new RemovePlayerBroadcast()
    //    {
    //        Slot = PlayerInfos[info.ID].ID
    //    };

    //    InstanceFinder.ServerManager.Broadcast(player);

    //    PlayerInfos.Remove(info.ID);
    //    PlayersReady.Remove(info.ID);

    //    //foreach(KeyValuePair<int, PlayerInfo> pInfos in PlayerInfos)
    //    //{
    //    //    PlayerInfo newInfo = PlayerInfos[pInfos.Value.ID];
    //    //    newInfo.Slot--;
    //    //    PlayerInfos[pInfos.Value.ID] = newInfo;
    //    //}

    //    if (!_startMatch && !_matchStarted)
    //    {
    //        _playersReady = 0;

    //        foreach (KeyValuePair<int, bool> playerReady in PlayersReady)
    //        {
    //            if (playerReady.Value)
    //                _playersReady++;
    //        }

    //        var playersReady = new PlayersReadyBroadcast()
    //        {
    //            PlayersReady = _playersReady,
    //            TotalPlayers = PlayersReady.Count
    //        };

    //        InstanceFinder.ServerManager.Broadcast(playersReady);
    //    }
    //}

    //public void PlayerReady(NetworkConnection conn, ReadyUpUI.ReadyBroadcast ready)
    //{
    //    if (!_startMatch && !_matchStarted)
    //    {
    //        PlayersReady[conn.ClientId] = ready.IsReady;

    //        _playersReady = 0;

    //        foreach (KeyValuePair<int, bool> player in PlayersReady)
    //        {
    //            if (player.Value)
    //                _playersReady++;
    //        }

    //        var playersReady = new PlayersReadyBroadcast()
    //        {
    //            PlayersReady = _playersReady,
    //            TotalPlayers = PlayersReady.Count
    //        };

    //        InstanceFinder.ServerManager.Broadcast(playersReady);

    //        if (_playersReady == PlayersReady.Count)
    //        {
    //            var start = new StartMatchBroadcast()
    //            {
    //                StartMatch = true,
    //                MatchStarted = false,
    //                MatchFinished = false
    //            };

    //            _startMatch = true;

    //            InstanceFinder.ServerManager.Broadcast(start);
    //        }
    //    }
    //}

    public void ConnectionChanged(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if(args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log("RUNNING CONN CHANGED");
            PlayerDisconnect(conn);
            
        }
    }

    public void PlayerDisconnect(NetworkConnection conn)
    {
        Debug.Log("Player ID disconnected: " + conn.ClientId);

        PlayerInfos.Remove(conn.ClientId);

        InstanceFinder.ServerManager.Despawn(conn.FirstObject);

        var player = new RemovePlayerBroadcast()
        {
            ID = conn.ClientId
        };

        InstanceFinder.ServerManager.Broadcast(player);
    }
}
