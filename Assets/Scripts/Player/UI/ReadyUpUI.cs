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

    private void Awake()
    {
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.PlayersReadyBroadcast>(UpdateReady);
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.StartMatchBroadcast>(MatchStart);
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.EndMatchBroadcast>(MatchEnd);
    }

    private void OnDestroy()
    {
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.PlayersReadyBroadcast>(UpdateReady);
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.StartMatchBroadcast>(MatchStart);
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.EndMatchBroadcast>(MatchEnd);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            var newPlayer = new ReadyBroadcast()
            {
                IsReady = false,
                IsDisc = false
            };

            InstanceFinder.ClientManager.Broadcast(newPlayer);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();      
    }

    public void MatchStart(MatchManager.StartMatchBroadcast info)
    {
       _readyVisual.SetActive(false);
    }

    public void MatchEnd(MatchManager.EndMatchBroadcast info)
    {
        if (info.MatchEnded)
        {
            _readyVisual.SetActive(true);
            _isReady = false;
            _selfReadyText.text = "Not Ready";
            _readyImage.color = Color.red;
            _winnerText.gameObject.SetActive(true);
            _winnerText.text = info.Winner + (" wins!");
        }
        else
        {
            _winnerText.text = "";
        }
    }

    public void ReadyUp()
    {

        if (_readyVisual.activeInHierarchy)
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
                IsReady = _isReady,
                IsDisc = false
            };

            InstanceFinder.ClientManager.Broadcast(ready);
        }
    }

    public void UpdateReady(MatchManager.PlayersReadyBroadcast info)
    {
        _playersReadyText.text = info.PlayersReady.ToString() + " / " + info.TotalPlayers.ToString();
    }
}
