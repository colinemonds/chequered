using System;

namespace Chequered.State
{
    public interface IStateObject<TStateName, out TState> where TState : IState
    {
        /// <summary>
        ///   The name of the object's current state.
        /// </summary>
        TStateName CurrentState { get; }

        /// <summary>
        ///   Change the state to another state. Leave() will be called on the current state, then Enter() will be called on the
        ///   new state.
        /// </summary>
        /// <param name="newState">The name of the state to change to.</param>
        void ChangeState(TStateName newState);

        /// <summary>
        ///   Return to the newest state that was recorded by PushState(). Leave() will be called on the current state, then
        ///   Enter() will be called on the prior state.
        /// </summary>
        /// <exception cref="InvalidOperationException">When there is no prior state.</exception>
        void PopState();

        /// <summary>
        ///   Change to another state, but remember the old state. You can later return to the old state using PopState(). Leave()
        ///   will be called on the old state, then Enter() will be called on the new state. You can push multiple states, and all
        ///   of them will be remembered.
        /// </summary>
        /// <param name="newState">The name of the state to change to.</param>
        void PushState(TStateName newState);

        /// <summary>
        ///   Do something with the current state, such as executing a method.
        /// </summary>
        /// <param name="runner">The action to do with the current State.</param>
        void RunState(Action<TState> runner);

        /// <summary>
        ///   Calculate something from the current state, for example by calling a method on it.
        /// </summary>
        /// <typeparam name="TResult">The result type of the called function.</typeparam>
        /// <param name="func">What you want to do with the state.</param>
        /// <returns>Whatever the function you gave returns.</returns>
        TResult RunState<TResult>(Func<TState, TResult> func);
    }
}