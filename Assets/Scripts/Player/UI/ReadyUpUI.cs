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
    public Image _readyImage;

    private void Awake()
    {
        InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.PlayersReadyBroadcast>(UpdateReady);
    }

    private void OnDestroy()
    {
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.PlayersReadyBroadcast>(UpdateReady);

        //if (_isReady)
        //ReadyUp();

        //var ready = new ReadyBroadcast()
        //{
        //    IsReady = false,
        //    IsDisc = true
        //};

        //InstanceFinder.ClientManager.Broadcast(ready);

        //var newPlayer = new ReadyBroadcast()
        //{
        //    IsReady = false,
        //    IsDisc = true
        //};

        //InstanceFinder.ClientManager.Broadcast(newPlayer);
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

    public void ReadyUp()
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

    public void UpdateReady(MatchManager.PlayersReadyBroadcast info)
    {
        _playersReadyText.text = info.PlayersReady.ToString() + " / " + info.TotalPlayers.ToString();
    }
}
