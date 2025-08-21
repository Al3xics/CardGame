using UnityEngine;
using Wendogo;

public class StealResourceHandler : MonoBehaviour
{
    public int _pickedTargetToSteal =-1;

    public void StealTargetPick(int pickedTarget)
    {
        _pickedTargetToSteal = pickedTarget;
    }

}
