using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EventTimer : MonoBehaviour
{
    public event Action<int> OnTick;           // remaining seconds each tick
    public event Action OnFinished;            // when it hits 0
    public event Action<bool> OnPausedChanged; // optional: observers know when pause toggles

    private CancellationTokenSource _cts;
    private bool _isPaused;
    private int _remaining;

    public bool IsPaused => _isPaused;
    public int RemainingSeconds => _remaining;

    public void StartTimer(int seconds)
    {
        CancelTimer();                 // stop any previous run
        _cts = new CancellationTokenSource();
        _isPaused = false;
        _ = RunTimerAsync(seconds, _cts.Token);
    }

    public void CancelTimer()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
        _isPaused = false;
    }

    // --- New API for pausing/unpausing ---
    public void PauseTimer() => SetPaused(true);
    public void ResumeTimer() => SetPaused(false);
    public void TogglePause() => SetPaused(!_isPaused);

    public void SetPaused(bool paused)
    {
        if (_isPaused == paused) return;
        _isPaused = paused;
        OnPausedChanged?.Invoke(_isPaused);
    }
    // -------------------------------------

    private async UniTaskVoid RunTimerAsync(int seconds, CancellationToken token)
    {
        _remaining = Mathf.Max(0, seconds);
        OnTick?.Invoke(_remaining); // immediate first update

        // Accumulate scaled deltaTime so we can pause mid-second without losing progress.
        float subSecond = 0f;

        while (_remaining > 0)
        {
            // Advance one frame on the Update loop; respects Time.timeScale
            await UniTask.Yield(PlayerLoopTiming.Update, token);
            if (token.IsCancellationRequested) return;

            if (_isPaused) continue; // while paused, no time passes for this timer

            subSecond += Time.deltaTime; // use Time.unscaledDeltaTime if you want it to ignore timeScale

            // Tick down every full second of accumulated time
            while (subSecond >= 1f && _remaining > 0)
            {
                subSecond -= 1f;
                _remaining--;
                OnTick?.Invoke(_remaining);
            }
        }

        OnFinished?.Invoke();
    }

    private void OnDisable() => CancelTimer(); // optional safety
}
