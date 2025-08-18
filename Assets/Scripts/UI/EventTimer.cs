using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EventTimer : MonoBehaviour
{
    public event Action<int> OnTick;   // remaining seconds each tick
    public event Action OnFinished;    // when it hits 0

    private CancellationTokenSource _cts;

    public void StartTimer(int seconds)
    {
        CancelTimer();                      // stop any previous run
        _cts = new CancellationTokenSource();
        _ = RunTimerAsync(seconds, _cts.Token);
    }

    public void CancelTimer()
    {
        if (_cts == null) return;
        _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }

    private async UniTaskVoid RunTimerAsync(int seconds, CancellationToken token)
    {
        int remaining = Mathf.Max(0, seconds);
        OnTick?.Invoke(remaining);          // immediate first update

        while (remaining > 0)
        {
            // Wait 1 second on the main threadfs player loop
            await UniTask.Delay(
                TimeSpan.FromSeconds(1),
                DelayType.DeltaTime,                // respects Time.timeScale
                PlayerLoopTiming.Update,
                token
            );

            if (token.IsCancellationRequested) return;

            remaining--;
            OnTick?.Invoke(remaining);
        }

        OnFinished?.Invoke();
    }

    private void OnDisable() => CancelTimer(); // optional safety
}
