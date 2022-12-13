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

    private void OnDestroy()
    {
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.ScoreBroadcast>(UpdateScore);
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.NewPlayerBroadCast>(NewPlayer);
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.RemovePlayerBroadcast>(RemovePlayer);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.ScoreBroadcast>(UpdateScore);
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.NewPlayerBroadCast>(NewPlayer);
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.RemovePlayerBroadcast>(RemovePlayer);
        _playerBars = new Dictionary<int, GameObject>();
        _changedSlot = false;
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
                slot._hasBar = true;
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
            slot._hasBar = true;
            bar._nameText.text = _newPlayer.Username;
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
        
        var slot = _playerSlots[barInfo._currentSlot].GetComponent<ScoreboardSlot>();
        slot._hasBar = false;
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

    //public void NewPlayer(MatchManager.NewPlayerBroadCast info)
    //{
    //    if(Owner == info.Conn)
    //    {
    //        foreach (KeyValuePair<int, MatchManager.PlayerInfo> player in info.CurrentPlayers)
    //        {
    //            //var bar = _playerBars[player.Value.Slot].GetComponent<PlayerBar>();

    //            var bar = Instantiate(_playerBar, _playerSlots[player.Value.Slot].transform).GetComponent<PlayerBar>();
    //            bar._nameText.text = player.Value.Username;
    //            bar._scoreText.text = player.Value.Score.ToString();
    //            bar._pingText.text = player.Value.ID.ToString();
    //            bar._currentSlot = player.Value.Slot;
    //            bar._score = player.Value.Score;
    //            bar._ID = player.Value.ID;
    //            bar._isActive = true;
    //            _playerBars.Add(bar.gameObject);

    //            if (player.Value.ID == Owner.ClientId)
    //            {
    //                ColorUtility.TryParseHtmlString("FF9494", out Color highlight);
    //                highlight.a = .02f;
    //                bar._barImage.color = highlight;                  
    //            }
    //        }
    //    }
    //    else
    //    {
    //        //var bar = _playerBars[info.Slot].GetComponent<PlayerBar>();
    //        var bar = Instantiate(_playerBar, _playerSlots[info.Slot].transform).GetComponent<PlayerBar>();
    //        bar.slotObject = _playerSlots[info.Slot].GetComponent<ScoreboardSlot>();
    //        bar._nameText.text = info.Username;
    //        bar._scoreText.text = "0";
    //        bar._pingText.text = info.ID.ToString();
    //        bar._score = 0;
    //        bar._currentSlot = info.Slot;
    //        bar._ID = info.ID;
    //        bar._isActive = true;
    //        _playerBars.Add(bar.gameObject);
    //    }
    //}

    //public void UpdatePlayerScore(MatchManager.ScoreBroadcast info)
    //{
    //    if (info.Reset)
    //    {
    //        foreach (GameObject playerBar in _playerBars)
    //        {
    //            var bar = playerBar.GetComponent<PlayerBar>();

    //            if (bar._isActive)
    //            {
    //                bar._scoreText.text = "0";
    //                bar._score = 0;
    //            }
    //        }
    //    }
    //    else
    //    {
    //        var barInfo = _playerBars[info.Slot].GetComponent<PlayerBar>();

    //        barInfo._scoreText.text = info.Score.ToString();
    //        barInfo._score = info.Score;

    //        MoveSlots(barInfo);
    //    }
    //}



    //public void RemovePlayer(MatchManager.RemovePlayerBroadcast slot)
    //{
    //    //var bar = _playerBars[slot.Slot].GetComponent<PlayerBar>();
    //    //bar._nameText.text = "";
    //    //bar._scoreText.text = "";
    //    //bar._pingText.text = "";
    //    //bar._isActive = false;
    //    //_playerBars.Remove(bar.gameObject);

    //    foreach(GameObject playerBar in _playerBars)
    //    {
    //        var bar = playerBar.GetComponent<PlayerBar>();

    //        if (bar._ID == slot.Slot)
    //            Destroy(bar.gameObject);
    //    }

    //    Debug.Log("Help Me");


    //    //foreach(var barInfo in _playerBars)
    //    //{
    //    //    var bar = barInfo.GetComponent<PlayerBar>();

    //    //    if (bar._ID == slot.Slot)
    //    //    {
    //    //        _barWorkspace = barInfo;
    //    //        Destroy(barInfo);
    //    //    }
    //    //}

    //    //_playerBars.Remove(_barWorkspace);
    //    //var removedBar = _barWorkspace.GetComponent<PlayerBar>();



    //    //for (int i = removedBar._currentSlot + 1; i < _playerSlots.Count; i++)
    //    //{
    //    //    var nextBar = _playerSlots[i].GetComponentInChildren<PlayerBar>();

    //    //    if (nextBar._isActive)
    //    //    {
    //    //        removedBar._currentSlot = nextBar._currentSlot;
    //    //        nextBar._currentSlot--;
    //    //    }                      
    //    //}

    //    //foreach (var barInfo in _playerBars)
    //    //{
    //    //   var bar = barInfo.GetComponent<PlayerBar>();
    //    //    if(bar._score < removedBar._score)
    //    //        bar._currentSlot--;
    //    //}



    //}

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
