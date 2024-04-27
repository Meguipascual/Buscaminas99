using System.Collections;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour {
    
    [SerializeField] private TMP_Text _timerText;
    
    private int _countdownSeconds;
    
    protected void Start() {
        _timerText.gameObject.SetActive(false);
    }
    
    public void StartTimer(int gameDurationSeconds) {
        _countdownSeconds = gameDurationSeconds;
        _timerText.gameObject.SetActive(true);
        _timerText.text = _countdownSeconds.ToString();
        StartCoroutine(UpdateTimerOneSecond());
    }

    private IEnumerator UpdateTimerOneSecond() {
        yield return new WaitForSeconds(1);
        _countdownSeconds--;
        if (_countdownSeconds <= 0) {
            _timerText.text = "Game finished";
        }
        else {
            var minutes = _countdownSeconds / 60;
            var seconds = _countdownSeconds % 60;
            _timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
            StartCoroutine(UpdateTimerOneSecond());
        }
    }
}