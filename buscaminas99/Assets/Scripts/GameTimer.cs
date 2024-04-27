using System;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour {
    
    [SerializeField] private TMP_Text _timerText;

    private long _startTimestamp;
    private long _endTimestamp;
    private int _countdownSeconds;
    
    protected void Start() {
        _timerText.gameObject.SetActive(false);
    }
    
    protected void Update() {
        if (_countdownSeconds > 0) {
            UpdateCountdown();
        }
    }
    
    public void StartTimer(long startTimestamp, int gameDurationSeconds) {
        _startTimestamp = startTimestamp;
        _endTimestamp = _startTimestamp + gameDurationSeconds;
        _countdownSeconds = gameDurationSeconds;
        _timerText.gameObject.SetActive(true);
        UpdateText();
    }

    private void UpdateCountdown() {
        var countdownSeconds = _endTimestamp - DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (countdownSeconds < _countdownSeconds) {
            _countdownSeconds = (int)countdownSeconds;
            UpdateText();
        }
    }

    private void UpdateText() {
        if (_countdownSeconds <= 0) {
            _timerText.text = "Game finished";
            return;
        }
        
        var minutes = _countdownSeconds / 60;
        var seconds = _countdownSeconds % 60;
        _timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
    }
}