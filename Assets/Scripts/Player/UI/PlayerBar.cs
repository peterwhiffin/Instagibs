using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerBar : MonoBehaviour
{
    public TMP_Text _nameText;
    public TMP_Text _scoreText;
    public TMP_Text _pingText;

    public Image _barImage;

    public int _currentSlot;
    public int _score;
    public bool _isActive;
    public int _ID;

    public ScoreboardSlot slotObject;

    public Scoreboard scoreboard;

    private void Start()
    {
        slotObject = transform.parent.GetComponent<ScoreboardSlot>();
        _currentSlot = slotObject.slot;
        scoreboard = GetComponentInParent<Scoreboard>();
    }

    private void OnDestroy()
    {
        
    }

    private void Update()
    {
        if (_currentSlot != slotObject.slot)
        {
            transform.SetParent(scoreboard._playerSlots[_currentSlot].transform);
            transform.localPosition = Vector3.zero;
            slotObject = transform.parent.GetComponent<ScoreboardSlot>();
        }

        //if(_currentSlot != 0)
        //{
        //    if(scoreboard._playerSlots[_currentSlot - 1].transform.childCount < 1)
        //    {
        //        _currentSlot--;
        //    }
        //}

        //if (_currentSlot != 0)
        //{
        //    if (scoreboard._playerSlots[_currentSlot - 1].GetComponent<ScoreboardSlot>().transform.childCount == 0)
        //    {
        //        _currentSlot--;
        //    }
        //}

        //if (_currentSlot < scoreboard._playerBars.Count - 1)
        //{
        //    if (scoreboard._playerSlots[_currentSlot + 1].GetComponentInChildren<PlayerBar>()._score > _score)
        //    {
        //        slotObject._hasBar = false;
        //        _currentSlot++;
        //    }
        //}
    }

    //public void BarCheck()
    //{
    //    if (_currentSlot != slotObject.slot)
    //    {
    //        transform.SetParent(scoreboard._playerSlots[_currentSlot].transform);
    //        transform.localPosition = Vector3.zero;
    //        slotObject = transform.parent.GetComponent<ScoreboardSlot>();
    //    }

    //    if (_currentSlot != 0)
    //    {
    //        if (!scoreboard._playerSlots[_currentSlot - 1].GetComponent<ScoreboardSlot>()._hasBar || scoreboard._playerSlots[_currentSlot - 1].GetComponentInChildren<PlayerBar>()._score < _score)
    //        {
    //            slotObject._hasBar = false;
    //            _currentSlot--;
    //        }
    //    }

    //    if (_currentSlot < scoreboard._playerBars.Count - 1)
    //    {
    //        if (scoreboard._playerSlots[_currentSlot + 1].GetComponentInChildren<PlayerBar>()._score > _score)
    //        {
    //            slotObject._hasBar = false;
    //            _currentSlot++;
    //        }
    //    }
    //}
}
