using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;
using FishNet.Broadcast;

public class Scoreboard : NetworkBehaviour
{
    public GameObject _playerBar;

    //private List<GameObject> _playerBars;
    public List<GameObject> _playerSlots;

    public Dictionary<int, GameObject> _playerBars;

    public GameObject _visual;
    public bool _isActive;
    public bool _changedSlot;
    public GameObject _barWorkspace;

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (base.IsOwner)
        {
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.ScoreBroadcast>(UpdateScore);
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.NewPlayerBroadCast>(NewPlayer);
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.RemovePlayerBroadcast>(RemovePlayer);
            _playerBars = new Dictionary<int, GameObject>();
            _changedSlot = false;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        if (base.IsOwner)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.ScoreBroadcast>(UpdateScore);
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.NewPlayerBroadCast>(NewPlayer);
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.RemovePlayerBroadcast>(RemovePlayer);
        }
    }

    public void NewPlayer(MatchManager.NewPlayerBroadCast _newPlayer)
    {
        if(Owner.ClientId == _newPlayer.ID)
        {
            foreach(KeyValuePair<int, MatchManager.PlayerInfo> player in _newPlayer.CurrentPlayers)
            {
                Debug.Log(_playerBars.Count);
                
                var bar = Instantiate(_playerBar, _playerSlots[_playerBars.Count].transform).GetComponent<PlayerBar>();
                var slot = _playerSlots[_playerBars.Count].GetComponent<ScoreboardSlot>();
                bar._ID = player.Value.ID;
                bar._nameText.text = player.Value.Username;
                bar._score = player.Value.Score;
                bar._scoreText.text = player.Value.Score.ToString();               
                _playerBars.Add(bar._ID, bar.gameObject);

                if (player.Value.ID == Owner.ClientId)
                {
                    ColorUtility.TryParseHtmlString("FF9494", out Color highlight);
                    highlight.a = .01f;
                    bar._barImage.color = highlight;                  
                }
                MoveSlots(bar);
            }
        }
        else
        {
            var bar = Instantiate(_playerBar, _playerSlots[_playerBars.Count].transform).GetComponent<PlayerBar>();
            var slot = _playerSlots[_playerBars.Count].GetComponent<ScoreboardSlot>();
            bar._nameText.text = _newPlayer.Username;
            bar._score = 0;
            bar._scoreText.text = "0";
            bar._ID = _newPlayer.ID;
            _playerBars.Add(bar._ID, bar.gameObject);
            MoveSlots(bar);
        }
    }

    public void RemovePlayer(MatchManager.RemovePlayerBroadcast player)
    {
        Debug.Log("RemovePlayer Running");
        var bar = _playerBars[player.ID];
        var barInfo = _playerBars[player.ID].GetComponent<PlayerBar>();
        int nextSlot = barInfo._currentSlot + 1;

        for (int i = nextSlot; i < _playerBars.Count; i++)
        {
            var nextbar = _playerSlots[i].GetComponentInChildren<PlayerBar>();
            nextbar._currentSlot--;
        }


        _playerBars.Remove(player.ID);
        
        Destroy(bar);
    }

    public void MoveSlots(PlayerBar barInfo)
    {
        if (barInfo._currentSlot != 0)
        {

            PlayerBar nextBarInfo = _playerSlots[barInfo._currentSlot - 1].GetComponentInChildren<PlayerBar>();

            if (barInfo._score > nextBarInfo._score)
            {
                barInfo._currentSlot--;
                nextBarInfo._currentSlot++;
                MoveSlots(barInfo);
            }
        }
    }

    public void UpdateScore(MatchManager.ScoreBroadcast score)
    {
        if (score.Reset)
        {
            foreach (KeyValuePair<int, GameObject> bar in _playerBars)
            {
                var barInfo = bar.Value.GetComponent<PlayerBar>();
                barInfo._score = 0;
                barInfo._scoreText.text = "0";
            }
        }
        else
        {
            var bar = _playerBars[score.ID].GetComponent<PlayerBar>();
            bar._score = score.Score;
            bar._scoreText.text = score.Score.ToString();
            MoveSlots(bar);
        }      
    }

    public void ToggleScoreboard(InputAction.CallbackContext context)
    {
        if (IsOwner)
        {
            if (context.started)
                _isActive = true;

            if (context.canceled)
                _isActive = false;

            _visual.SetActive(_isActive);
        }
    }
}
