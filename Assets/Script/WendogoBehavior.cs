using UnityEngine;

public class WendogoBehavior : PlayerBehavior
{
    public override bool CanAct(Cycle currentCycle)
    {
        return currentCycle == Cycle.Night;
    }
}
