using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chequered.State
{
    /// <summary>
    ///   A state manager allows your application to easily deal with situations where an object might be in one of different
    ///   states. Give each state a name, then implement the IState interface for each object state, factoring all logic out of
    ///   the main object. Then all that's left to do is making the main object a shim that calls into the current state at any
    ///   given time (using the Run() method).
    /// </summary>
    public class StateManager<TStateName, TState> : IStateObject<TStateName, TState> where TState : IState
    {
        private readonly Stack<TStateName> _priorStates;
        private readonly ReadOnlyDictionary<TStateName, TState> _states;

        /// <summary>Creates a new instance.</summary>
        /// <param name="states">The states of your object.</param>
        /// <param name="startState">
        ///   The name of the state that your object starts in. Enter() will be called on the named state on
        ///   construction.
        /// </param>
        public StateManager(IDictionary<TStateName, TState> states, TStateName startState)
        {
            _states = new ReadOnlyDictionary<TStateName, TState>(states);
            _priorStates = new Stack<TStateName>();
            CurrentState = startState;
            _states[CurrentState].Enter();
        }

        /// <summary>
        ///   The name of the object's current state.
        /// </summary>
        public TStateName CurrentState { get; private set; }

        /// <summary>
        ///   Change the state to another state. Leave() will be called on the current state, then Enter() will be called on the
        ///   new state.
        /// </summary>
        /// <param name="newState">The name of the state to change to.</param>
        public void ChangeState(TStateName newState)
        {
            _states[CurrentState].Leave();
            CurrentState = newState;
            _states[CurrentState].Enter();
        }

        /// <summary>
        ///   Return to the newest recorded prior state that was remembered by PushState(). Leave() will be called on the current
        ///   state, then Enter() will be called on the prior state.
        /// </summary>
        /// <exception cref="InvalidOperationException">When there is no prior state.</exception>
        public void PopState()
        {
            if (!_priorStates.Any())
            {
                throw new InvalidOperationException("no prior state recorded");
            }
            ChangeState(_priorStates.Pop());
        }

        /// <summary>
        ///   Change to another state, but remember the old state. You can later return to the old state using PopState(). Leave()
        ///   will be called on the old state, then Enter() will be called on the new state. You can push multiple states, and all
        ///   of them will be remembered.
        /// </summary>
        /// <param name="newState">The name of the state to change to.</param>
        public void PushState(TStateName newState)
        {
            _priorStates.Push(CurrentState);
            ChangeState(newState);
        }

        /// <summary>
        ///   Do something with the current state, such as executing a method.
        /// </summary>
        /// <param name="runner">The action to do with the current State.</param>
        public void RunState(Action<TState> runner)
        {
            runner(_states[CurrentState]);
        }

        /// <summary>
        /// Calculate something from the current state, for example by calling a method on it.
        /// </summary>
        /// <typeparam name="TResult">The result type of the called function.</typeparam>
        /// <param name="func">What you want to do with the state.</param>
        /// <returns>Whatever the function you gave returns.</returns>
        public TResult RunState<TResult>(Func<TState, TResult> func)
        {
            return func(_states[CurrentState]);
        }
    }
}