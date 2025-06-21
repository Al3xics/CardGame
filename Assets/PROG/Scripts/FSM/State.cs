using UnityEngine;

namespace Wendogo
{
    /// <summary>
    /// Represents an abstract base class for the states in a state machine.
    /// Each state is associated with a specific state machine and provides methods
    /// to handle entry, execution (tick), and exit logic.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the state machine. It must inherit from <see cref="StateMachine{T}"/>.
    /// </typeparam>
    public abstract class State<T> : IState where T : StateMachine<T>
    {
        /// <summary>
        /// Represents an abstract state machine responsible for managing states in a finite state machine pattern.
        /// </summary>
        /// <typeparam name="T">The specific type of the state machine inheriting from this base class.</typeparam>
        protected T StateMachine;

        /// <summary>
        /// Represents an abstract state in a state machine.
        /// All states inherit from this base class and define behavior for entering, ticking, and exiting the state.
        /// </summary>
        /// <typeparam name="T">The type of the state machine this state belongs to, constrained to a StateMachine of the same type.</typeparam>
        protected State(T stateMachine)
        {
            StateMachine = stateMachine;
        }

        /// <summary>
        /// Custom log method. If <see cref="StateMachine.showDebugLogs"/> in <see cref="StateMachine"/> is true,
        /// the log will be shown; otherwise not.
        /// </summary>
        /// <param name="message">Message you want to log.</param>
        protected void Log(string message)
        {
            if (StateMachine.ShowDebugLogs)
                Debug.Log(message);
        }

        /// <summary>
        /// Custom log method. If <see cref="StateMachine.showDebugLogs"/> in <see cref="StateMachine"/> is true,
        /// the log will be shown; otherwise not.
        /// </summary>
        /// <param name="message">Message you want to log warning.</param>
        protected void LogWarning(string message)
        {
            if (StateMachine.ShowDebugLogs)
                Debug.LogWarning(message);
        }

        /// <summary>
        /// Custom log method. If <see cref="StateMachine.showDebugLogs"/> in <see cref="StateMachine"/> is true,
        /// the log will be shown; otherwise not.
        /// </summary>
        /// <param name="message">Message you want to log error.</param>
        protected void LogError(string message)
        {
            if (StateMachine.ShowDebugLogs)
                Debug.LogError(message);
        }

        /// <summary>
        /// Invoked when entering the state.
        /// Performs operations needed to initialize the state when it becomes active.
        /// </summary>
        public virtual void OnEnter()
        {
            Log($"---------- Enter {GetType()}");
        }

        /// <summary>
        /// Represents an action performed on each update or frame while the current state is active.
        /// This method is intended to be overridden in derived classes to define state-specific behavior
        /// that occurs consistently during updates in the game loop.
        /// </summary>
        /// <remarks>
        /// Typical use cases for this method include polling or processing input, executing per-frame
        /// logic, or monitoring certain conditions relevant to the state.
        /// </remarks>
        public virtual void OnTick()
        {
            Log($"----- Tick {GetType()} -----");
        }

        /// <summary>
        /// This method is invoked when the state exits.
        /// It serves as an entry point for cleanup and state-exit logic specific to the current state.
        /// Override this method in derived classes to implement custom behavior needed when transitioning away from the state.
        /// </summary>
        public virtual void OnExit()
        {
            Log($"Exit {GetType()} ----------");
        }
    }
}