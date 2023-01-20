using FishNet;
using FishNet.Broadcast;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FishNet.Object;

public class ReadyUpUI : NetworkBehaviour
{
    public struct ReadyBroadcast : IBroadcast
    {
        public bool IsReady;
        public bool IsDisc;
    }


    public bool _isReady;
    
    public TMP_Text _playersReadyText;
    public TMP_Text _selfReadyText;
    public TMP_Text _winnerText;
    public Image _readyImage;

    public GameObject _readyVisual;

    public override void OnStartClient()
    {
        base.OnStartClient();
              
        if (base.IsOwner)
        {
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.PlayersReadyBroadcast>(UpdateReady);
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.StartMatchBroadcast>(MatchStart);
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.EndMatchBroadcast>(MatchEnd);
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.CheckMatchStatus>(UpdateMatchStatus);
            _winnerText.text = "";
            _isReady = false;
        }
    }

    public void UpdateMatchStatus(MatchManager.CheckMatchStatus status)
    {
        if (status.StartMatch || status.MatchStarted)
        {
            _readyVisual.SetActive(false);
            _winnerText.text = " ";
        }
        else if(!status.StartMatch && !status.MatchStarted)
        {
            _readyVisual.SetActive(true);
            _isReady = false;
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        if (base.IsOwner)
        {
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.PlayersReadyBroadcast>(UpdateReady);
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.StartMatchBroadcast>(MatchStart);
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.EndMatchBroadcast>(MatchEnd);
            InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.CheckMatchStatus>(UpdateMatchStatus);
        }
    }

    public void MatchStart(MatchManager.StartMatchBroadcast info)
    {
        _winnerText.text = " ";
        //_readyVisual.SetActive(false);       
    }

    public void MatchEnd(MatchManager.EndMatchBroadcast info)
    {
        _readyVisual.SetActive(true);
        _isReady = false;
        _selfReadyText.text = "Not Ready";
        _readyImage.color = Color.red;
        _winnerText.text = info.Winner + (" wins!");
        
        var ready = new ReadyBroadcast()
        {
            IsReady = _isReady
        };

        InstanceFinder.ClientManager.Broadcast(ready);
    }

    public void ReadyUp()
    {
        if (_readyVisual.activeInHierarchy && base.IsOwner)
        {
            _isReady = !_isReady;

            if (_isReady)
            {
                _selfReadyText.text = "Ready";
                _readyImage.color = Color.green;
            }
            else
            {
                _selfReadyText.text = "Not Ready";
                _readyImage.color = Color.red;
            }


            var ready = new ReadyBroadcast()
            {
                IsReady = _isReady
            };

            InstanceFinder.ClientManager.Broadcast(ready);
        }
    }

    public void UpdateReady(MatchManager.PlayersReadyBroadcast info)
    {
        _playersReadyText.text = info.PlayersReady.ToString() + " / " + info.TotalPlayers.ToString();
    }
}
