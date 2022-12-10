using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FishNet;

public class Scoreboard : NetworkBehaviour
{
    public GameObject _playerBar;

    public List<GameObject> _playerBars;
    public List<GameObject> _playerSlots;

    public GameObject _visual;
    public bool _isActive;

    private void Awake()
    {
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.ScoreBroadcast>(UpdatePlayerScore);
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.NewPlayerBroadCast>(NewPlayer);
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.RemovePlayerBroadcast>(RemovePlayer);
    }

    private void OnDestroy()
    {
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.ScoreBroadcast>(UpdatePlayerScore);
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.NewPlayerBroadCast>(NewPlayer);
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.RemovePlayerBroadcast>(RemovePlayer);
    }

    public void NewPlayer(MatchManager.NewPlayerBroadCast info)
    {
        if(Owner == info.Conn)
        {
            foreach (KeyValuePair<int, MatchManager.PlayerInfo> player in info.CurrentPlayers)
            {
                var bar = _playerBars[player.Value.Slot].GetComponent<PlayerBar>();
                bar._nameText.text = player.Value.Username;
                bar._scoreText.text = player.Value.Score.ToString();
                bar._currentSlot = player.Value.Slot;
                bar._score = player.Value.Score;
                bar._isActive = true;
                
                if (player.Value.ID == Owner.ClientId)
                {
                    ColorUtility.TryParseHtmlString("FF9494", out Color highlight);
                    highlight.a = .02f;
                    bar._barImage.color = highlight;                  
                }
            }
        }
        else
        {
            var bar = _playerBars[info.Slot].GetComponent<PlayerBar>();
            bar._nameText.text = info.Username;
            bar._scoreText.text = "0";
            bar._score = 0;
            bar._currentSlot = info.Slot;
            bar._isActive = true;
        }
    }

    public void UpdatePlayerScore(MatchManager.ScoreBroadcast info)
    {
        if (info.Reset)
        {
            foreach (GameObject playerBar in _playerBars)
            {
                var bar = playerBar.GetComponent<PlayerBar>();
                
                if (bar._isActive)
                {
                    bar._scoreText.text = "0";
                    bar._score = 0;
                }
            }
        }
        else
        {
            var barInfo = _playerBars[info.Slot].GetComponent<PlayerBar>();

            barInfo._scoreText.text = info.Score.ToString();
            barInfo._score = info.Score;

            MoveSlots(barInfo);
        }
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

    public void RemovePlayer(MatchManager.RemovePlayerBroadcast slot)
    {
        var bar = _playerBars[slot.Slot].GetComponent<PlayerBar>();
        bar._nameText.text = "";
        bar._scoreText.text = "";
        bar._pingText.text = "";
        bar._isActive = false;

        for(int i = bar._currentSlot; i < _playerSlots.Count; i++)
        {
            var nextBar = _playerSlots[i].GetComponentInChildren<PlayerBar>();
            nextBar._currentSlot--;
        }   

        bar._currentSlot = 9;
    }

    public void ToggleScoreboard(InputAction.CallbackContext context)
    {
        if (context.started)
            _isActive = true;

        if(context.canceled)
            _isActive = false;

        _visual.SetActive(_isActive);
    }
}
