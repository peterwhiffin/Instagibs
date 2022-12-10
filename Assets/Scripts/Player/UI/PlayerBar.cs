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

    public ScoreboardSlot slotObject;

    public Scoreboard scoreboard;

    private void Update()
    {
        if (_currentSlot != slotObject.slot)
        {
            transform.SetParent(scoreboard._playerSlots[_currentSlot].transform);
            transform.localPosition = Vector3.zero;
            slotObject = transform.parent.GetComponent<ScoreboardSlot>();
        }
    }
}
