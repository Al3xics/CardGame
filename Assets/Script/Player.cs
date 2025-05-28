using UnityEngine;

public class Player : MonoBehaviour
{
    public string playerName;
    public PlayerBehavior behavior;

    public void SetReady()
    {
        behavior.SetReady();
    }

    public bool CanAct(Cycle currentCycle)
    {
        return behavior.CanAct(currentCycle);
    }

    public bool IsReady()
    {
        return behavior.IsReady;
    }

    public void ResetReady()
    {
        behavior.ResetReady();
    }
}
