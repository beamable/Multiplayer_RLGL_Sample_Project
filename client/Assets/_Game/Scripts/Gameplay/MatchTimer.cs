using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class MatchTimer : NetworkBehaviour
{
    public enum MatchTimerState {Stopped, Running, Paused}

    [Networked] public MatchTimerState TimerState { get; private set; } = MatchTimerState.Stopped;
    [Networked] public float TimeLeft { get; private set; }

    public event EventHandler TimeUpEvent;

    [SerializeField] private HUDTimerUI _hudTimerUI;

    public override void Spawned()
    {
        base.Spawned();

        if (_hudTimerUI == null)
            _hudTimerUI = FindObjectOfType<HUDTimerUI>(true);
    }

    public override void FixedUpdateNetwork()
    {
        if (TimerState == MatchTimerState.Running)
        {
            TimeLeft = Mathf.Clamp(TimeLeft - Runner.DeltaTime, 0.0f, _maxMatchTime);
            SetTimerText(TimeLeft);

            if (TimeLeft <= 0)
                OnTimeUp();
        }
    }

    private float _maxMatchTime = 0.0f;
    public void StartTimer(float matchTime)
    {
        _maxMatchTime = matchTime;

        SetTimerText(matchTime);

        if (TimerState != MatchTimerState.Running)
            TimerState = MatchTimerState.Running;
    }

    public void PauseTimer()
    {
        TimerState = MatchTimerState.Paused;
    }

    public void StopTimer()
    {
        TimeLeft = 0;
        TimerState = MatchTimerState.Stopped;
    }

    public void ResetTimer(float matchTime)
    {
        _maxMatchTime = matchTime;

        StopTimer();
        TimeLeft = matchTime;
        SetTimerText(matchTime);
    }

    private void OnTimeUp()
    {
        StopTimer();
        TimeUpEvent?.Invoke(this, EventArgs.Empty);
    }

    public void SetTimerText(float matchTime)
    {
        if (_hudTimerUI == null)
            _hudTimerUI = FindObjectOfType<HUDTimerUI>(true);

        if (_hudTimerUI != null)
            _hudTimerUI.SetTimerText(matchTime);
    }
}
