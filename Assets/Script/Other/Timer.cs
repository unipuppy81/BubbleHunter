using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public enum PlayState
    {
        RUN,
        STOP,
        PAUSE
    }
    public PlayState CurrentState
    {
        get
        {
            return currentState;
        }
    }

    PlayState currentState = PlayState.STOP;
    private float step;
    private float maxTimer;
    private float timer;
    private float preTimer;

    public delegate void EndTimer();
    public delegate void EndEverySeconds(int secs);
    public delegate void UpdatingPercentage(float percent);

    EndTimer endTimerFunction;
    EndEverySeconds endEverySeconds;
    UpdatingPercentage updating;

    private void FixedUpdate()
    {
        switch (CurrentState)
        {
            case PlayState.RUN:
                timer -= Time.fixedDeltaTime * step;

                if (updating != null)
                    updating(1 - timer * 1.0f / maxTimer);

                if (Mathf.Abs(timer - preTimer) >= 1)
                {
                    preTimer = timer;
                    if (endEverySeconds != null)
                        endEverySeconds(Mathf.RoundToInt(preTimer));
                }

                if (timer < 0)
                {
                    timer = maxTimer;
                    currentState = PlayState.STOP;

                    if (endTimerFunction != null)
                        endTimerFunction();
                }
                break;
            case PlayState.PAUSE:
                break;
            case PlayState.STOP:
                preTimer = timer = maxTimer;
                break;
        }
    }

    public void StartTimerUpdateSeconds(float _maxTimer, EndTimer endFunc, EndEverySeconds endSecs = null)
    {
        step = 1;
        timer = maxTimer = _maxTimer;
        endTimerFunction = endFunc;
        endEverySeconds = endSecs;
        updating = null;
        currentState = PlayState.RUN;
    }

    public void StartTimerUpdatePercentage(float _maxTimer, EndTimer endFunc, UpdatingPercentage updatingFunc = null)
    {
        step = 1;
        timer = maxTimer = _maxTimer;
        endTimerFunction = endFunc;
        endEverySeconds = null;
        updating = updatingFunc;
        currentState = PlayState.RUN;
    }

    public void StopTimer()
    {
        currentState = PlayState.STOP;
        endTimerFunction = null;
        endEverySeconds = null;
    }

    public void PauseTimer()
    {
        if (currentState == PlayState.RUN)
            currentState = PlayState.PAUSE;

    }

    public void ContinueTimer()
    {
        if (currentState == PlayState.PAUSE)
            currentState = PlayState.RUN;
    }
}
