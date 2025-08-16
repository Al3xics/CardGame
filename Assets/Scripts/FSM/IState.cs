namespace Wendogo
{
    /// <summary>
    /// Defines the structure of a state in a state machine.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Called when the state is entered.
        /// This method is intended to perform any initialization, setup, or preparation
        /// required when the state becomes active within the state machine.
        /// </summary>
        public void OnEnter();

        /// <summary>
        /// Represents an action performed on every update cycle while the current state is active.
        /// This method is executed during the state machine's update loop and is intended to encapsulate
        /// state-specific behavior that runs consistently on each frame.
        /// </summary>
        /// <remarks>
        /// Derived classes should override this method to implement their specific per-frame logic.
        /// Common use cases include handling input, performing calculations, or evaluating conditions relevant
        /// to the state. This method is called automatically by the state machine while the state is active.
        /// </remarks>
        public void OnTick();

        /// <summary>
        /// Handles the cleanup and relevant logic when a state is exited.
        /// This method is called during the transition out of the current state in the state machine.
        /// Override this method in derived classes to execute specific actions or resource deallocation
        /// required when leaving the state.
        /// </summary>
        public void OnExit();
    }
}