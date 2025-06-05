using UnityEngine;
using Wendogo;

namespace Wendogo
{
    public class PlayerControllerSM : StateMachine<PlayerControllerSM>
    {
        protected override State<PlayerControllerSM> GetInitialState()
        {
            PCInputState state = new PCInputState(this);

            return state;

        }
    }
}