using FishNet.Object;
using System.Collections.Generic;
using FishNet.Connection;
using FishNet;
using FishNet.Broadcast;
using FishNet.Transporting;

public class MatchManager : NetworkBehaviour
{
    public struct PlayerInfo
    {
        public string Username;
        public int Score;
        public int Slot;
        public int ID;
    }

    public struct ScoreBroadcast : IBroadcast
    {
        public int Score;
        public int Slot;
    }

    public struct NewPlayerBroadCast : IBroadcast
    {
        public string Username;
        public int Slot;
        public int ID;
        public NetworkConnection Conn;
        public Dictionary<int, PlayerInfo> CurrentPlayers;
    }

    public struct RemovePlayerBroadcast : IBroadcast
    {
        public int Slot;
    }

    public struct PlayersReadyBroadcast : IBroadcast
    {
        public int PlayersReady;
        public int TotalPlayers;
    }

    public int _playersReady;

    public Dictionary<int, PlayerInfo> PlayerInfos;
    public Dictionary<int, bool> PlayersReady;



    public override void OnStartServer()
    {
        base.OnStartServer();
        InstanceFinder.ServerManager.RegisterBroadcast<Player.PlayerInfo>(NewPlayer);
        InstanceFinder.ServerManager.RegisterBroadcast<Player.ScoreBroadcast>(UpdateScore);
        InstanceFinder.ServerManager.RegisterBroadcast<Player.DisconnectBroadcast>(PlayerDisconnect);
        InstanceFinder.ServerManager.RegisterBroadcast<ReadyUpUI.ReadyBroadcast>(PlayerReady);

        PlayerInfos = new Dictionary<int, PlayerInfo>();
        PlayersReady = new Dictionary<int, bool>();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        InstanceFinder.ServerManager.UnregisterBroadcast<Player.PlayerInfo>(NewPlayer);
        InstanceFinder.ServerManager.UnregisterBroadcast<Player.ScoreBroadcast>(UpdateScore);
        InstanceFinder.ServerManager.UnregisterBroadcast<Player.DisconnectBroadcast>(PlayerDisconnect);
        InstanceFinder.ServerManager.UnregisterBroadcast<ReadyUpUI.ReadyBroadcast>(PlayerReady);
    }

    public void NewPlayer(NetworkConnection conn, Player.PlayerInfo info)
    {
        var newInfo = new PlayerInfo();
        var broadcastInfo = new NewPlayerBroadCast();
        newInfo.Username = broadcastInfo.Username = info.Username;
        newInfo.Score = 0;
        newInfo.Slot = broadcastInfo.Slot = PlayerInfos.Count;
        newInfo.ID = broadcastInfo.ID = conn.ClientId;
        broadcastInfo.Conn = conn;
        broadcastInfo.CurrentPlayers = PlayerInfos;
        PlayerInfos.Add(conn.ClientId, newInfo);
        PlayersReady.Add(conn.ClientId, false);

        InstanceFinder.ServerManager.Broadcast(broadcastInfo);        
    }

    public void UpdateScore(NetworkConnection conn, Player.ScoreBroadcast SB)
    {
        PlayerInfo info = PlayerInfos[SB.ID];
        info.Score++;
        PlayerInfos[SB.ID] = info;

        var newB = new ScoreBroadcast()
        {
            Score = info.Score,
            Slot = info.Slot
        };

        InstanceFinder.ServerManager.Broadcast(newB);
    }

    public void PlayerDisconnect(NetworkConnection conn, Player.DisconnectBroadcast info)
    {
        var player = new RemovePlayerBroadcast()
        {
            Slot = PlayerInfos[info.ID].Slot
        };

        InstanceFinder.ServerManager.Broadcast(player);

        PlayerInfos.Remove(info.ID);
        PlayersReady.Remove(info.ID);

        _playersReady = 0;

        foreach (KeyValuePair<int, bool> playerReady in PlayersReady)
        {
            if (playerReady.Value)
                _playersReady++;
        }

        var playersReady = new PlayersReadyBroadcast()
        {
            PlayersReady = _playersReady,
            TotalPlayers = PlayersReady.Count
        };

        InstanceFinder.ServerManager.Broadcast(playersReady);
    }

    public void PlayerReady(NetworkConnection conn, ReadyUpUI.ReadyBroadcast ready)
    {
        PlayersReady[conn.ClientId] = ready.IsReady;

        _playersReady = 0;

        foreach (KeyValuePair<int, bool> player in PlayersReady)
        {
            if (player.Value)
                _playersReady++;
        }

        var playersReady = new PlayersReadyBroadcast()
        {
            PlayersReady = _playersReady,
            TotalPlayers = PlayersReady.Count
        };

        InstanceFinder.ServerManager.Broadcast(playersReady);
    }
}
