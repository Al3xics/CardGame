using UnityEngine;

public class SurvivorBehavior : PlayerBehavior
{
    public override bool CanAct(Cycle currentCycle)
    {
        return currentCycle == Cycle.Day;
    }
}
