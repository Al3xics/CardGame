using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents a generic state machine that functions as a base class for implementing specific state machines.
    /// This class manages the current state of the machine and supports transitioning between different states.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the implementing state machine. This must inherit from <see cref="StateMachine{T}"/>.
    /// </typeparam>
    public abstract class StateMachine<T> : MonoBehaviour where T : StateMachine<T>
    {
        
        /// <summary>
        /// Show or hide the debug logs related to the <see cref="StateMachine{T}"/>.
        /// </summary>
        [Header("Debug")]
        [Tooltip("Show or hide the debug logs related to the StateMachine.")]
        [SerializeField] private bool showDebugLogs = true;
        
        /// <summary>
        /// See <see cref="showDebugLogs"/>.
        /// </summary>
        public bool ShowDebugLogs => showDebugLogs;
        
        /// <summary>
        /// Represents the current state of the state machine.
        /// This property holds the active state instance of type <see cref="State{T}"/> for the state machine.
        /// The state machine transitions between states by updating this property.
        /// </summary>
        /// <remarks>
        /// The <c>CurrentState</c> is automatically set to the initial state when the state machine starts,
        /// as defined by the <see cref="StateMachine{T}.GetInitialState"/> method. State transitions can
        /// be triggered by invoking the <see cref="StateMachine{T}.ChangeState{TState}"/> method,
        /// which updates the <c>CurrentState</c> and <c>PreviousState</c> accordingly and calls the appropriate lifecycle
        /// methods (<see cref="State{T}.OnExit"/>, <see cref="State{T}.OnEnter"/>) on the involved states.
        /// </remarks>
        private State<T> CurrentState { get; set; }
        
        /// <summary>
        /// Represents the previous state of the state machine.
        /// This property holds the previous state instance of type <see cref="State{T}"/>, who was active before
        /// the <see cref="CurrentState"/>.
        /// </summary>
        /// <remarks>
        /// State transitions can be triggered by invoking the <see cref="StateMachine{T}.ChangeState{TState}"/> method,
        /// which updates the <c>CurrentState</c> and <c>PreviousState</c> accordingly and calls the appropriate lifecycle
        /// methods (<see cref="State{T}.OnExit"/>, <see cref="State{T}.OnEnter"/>) on the involved states.
        /// </remarks>
        public State<T> PreviousState { get; private set; }

        /// <summary>
        /// A collection of state instances mapped by their respective types.
        /// This dictionary is used internally by the state machine to manage and
        /// retrieve the registered states for transitions or operations.
        /// </summary>
        private readonly Dictionary<Type, State<T>> _states = new();

        /// <summary>
        /// Initializes the state machine and transitions to the initial state.
        /// This method calls the <see cref="GetInitialState"/> to retrieve the starting state
        /// of the state machine. It then transitions to that state by invoking its <see cref="State{T}.OnEnter"/> method.
        /// If overridden in derived classes, ensure to call the base implementation to properly initialize the state machine.
        /// </summary>
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

        /// <summary>
        /// Adds a state to the state machine. This method allows for dynamic
        /// addition of valid states to the state machine during or before execution.
        /// </summary>
        /// <param name="state">The state instance to be added to the state machine. The state must implement
        /// the <see cref="State{T}"/> for the relevant state machine type.</param>
        protected void AddState(State<T> state)
        {
            _states.Add(state.GetType(), state);
        }

        /// <summary>
        /// Transitions the state machine to a new state of the specified type.
        /// The current state's <c>OnExit</c> method will be called, and the new
        /// state's <c>OnEnter</c> method will be invoked. This method assumes
        /// that the target state has already been added to the state machine
        /// using <see cref="AddState"/>.
        /// </summary>
        /// <typeparam name="TState">The type of the new state to transition into. Must inherit from <c>State<T /></c>.</typeparam>
        public void ChangeState<TState>() where TState : State<T>
        {
            CurrentState?.OnExit();
            PreviousState = CurrentState;
            CurrentState = GetState<TState>();
            CurrentState?.OnEnter();
        }

        /// <summary>
        /// Retrieves a specific state instance of the given type from the state machine.
        /// This method fetches a state that has been previously registered using <see cref="AddState"/>.
        /// Use this method when you need access to a concrete type of state for manipulation or inspection.
        /// </summary>
        /// <returns>The requested state instance of type <typeparamref name="TState"/>.</returns>
        private State<T> GetState<TState>()
        {
            return _states[typeof(TState)];
        }
        
        /// <summary>
        /// Retrieves a concrete state of the specified type from the state machine.
        /// The method ensures that the returned state matches the specified generic type
        /// and casts the retrieved state to this type, or returns null if the cast is invalid.
        /// This allows direct interaction with a specific state's behavior within the state machine.
        /// </summary>
        /// <returns>The instance of the state cast to the specified type, or null if the cast fails.</returns>
        protected TState GetConcreteState<TState>() where TState : State<GameStateMachine>
        {
            return GetState<TState>() as TState;
        }
    }
}