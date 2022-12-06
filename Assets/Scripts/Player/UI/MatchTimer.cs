using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;

public class MatchTimer : NetworkBehaviour
{
    public bool _matchStarted;
    private double _matchTime;
    public TMP_Text _timerText;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (base.IsOwner)
        {
            _matchStarted = true;
            _matchTime = 300;
        }
    }

    void Update()
    {
        if (_matchStarted)
            RunMatchTimer();

    }

    public void RunMatchTimer()
    {
        _matchTime -= base.TimeManager.TickDelta;
        float floatedTime = ((float)_matchTime);
        
        var minutes = Mathf.FloorToInt(floatedTime / 60);
        var seconds = Mathf.FloorToInt(floatedTime % 60);

        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
