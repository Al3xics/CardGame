using UnityEngine;

public abstract class PlayerBehavior : MonoBehaviour
{
    protected bool isReady = false;
    public bool IsReady => isReady;

    public abstract bool CanAct(Cycle currentCycle);

    public virtual void SetReady()
    {
        isReady = true;
    }

    public virtual void ResetReady()
    {
        isReady = false;
    }
}
