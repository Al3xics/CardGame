using UnityEngine;

namespace Wendogo
{
    public abstract class State<T> : IState where T : StateMachine<T>
    {
        protected T StateMachine;

        protected State(T stateMachine)
        {
            StateMachine = stateMachine;
        }
        
        public virtual void OnEnter()
        {
            Debug.Log($"Enter {GetType()}");
        }

        public virtual void OnTick()
        {
            Debug.Log($"Tick {GetType()}");
        }

        public virtual void OnExit()
        {
            Debug.Log($"Exit {GetType()}");
        }
    }
}