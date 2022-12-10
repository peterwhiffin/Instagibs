using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;
using FishNet;

public class MatchTimer : NetworkBehaviour
{
    public TMP_Text _timerText;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            InstanceFinder.ClientManager.RegisterBroadcast<MatchManager.TimeBroadcast>(UpdateTime);
        }
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        InstanceFinder.ClientManager.UnregisterBroadcast<MatchManager.TimeBroadcast>(UpdateTime);
    }

    public void UpdateTime(MatchManager.TimeBroadcast time)
    {
        _timerText.text = time.Time;
    }
}
