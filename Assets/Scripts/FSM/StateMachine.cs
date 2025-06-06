using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    public abstract class StateMachine<T> : MonoBehaviour where T : StateMachine<T>
    {
        private State<T> CurrentState { get; set; }
        private readonly Dictionary<Type, State<T>> _states = new();

        protected virtual void Start()
        {
            CurrentState = GetInitialState();
            CurrentState?.OnEnter();
        }

        /// <summary>
        /// Retrieves the initial state for the state machine.
        /// This method should be overridden in any derived class to define
        /// the initial state of the state machine. The returned state will be
        /// set as the starting state when the state machine begins execution.
        /// You can also add all your states to the state machine in this method with <see cref="AddState"/>./>
        /// </summary>
        /// <returns>The initial state to be used by the state machine.</returns>
        protected abstract State<T> GetInitialState();

        protected void AddState(State<T> state)
        {
            _states.Add(state.GetType(), state);
        }

        public void ChangeState<TState>() where TState : State<T>
        {
            CurrentState?.OnExit();
            CurrentState = GetState<TState>();
            CurrentState?.OnEnter();
        }

        private State<T> GetState<TState>()
        {
            return _states[typeof(TState)];
        }
        
        protected TState GetConcreteState<TState>() where TState : State<GameStateMachine>
        {
            return GetState<TState>() as TState;
        }
    }
}