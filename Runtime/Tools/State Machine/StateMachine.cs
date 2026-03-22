using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BlueMuffinGames.Tools.StateMachine
{
    public class StateMachine : MonoBehaviour
    {
        [SerializeField] private List<State> _states = new();
        [SerializeField] private bool _allowExternalStates = false;
        [SerializeField] private int _initialStateIndex = 0;
        [SerializeField] private bool _printWarningsAndErrors = false;

        public IReadOnlyDictionary<string, State> StateRegistry => _stateRegistry;
        public State CurrentState { get; private set; }

        private Dictionary<string, State> _stateRegistry = new();

        public event Action<State> OnStateExited = delegate { };
        public event Action<State> OnStateEntered = delegate { };

        private bool _initialized = false;

        public virtual void ChangeState(string stateName)
        {
            if (_stateRegistry.TryGetValue(stateName, out State state))
            {
                ChangeState(state);
            }
            else LogWarning($"State {stateName} is not registered in StateMachine {name}");
        }

        public virtual void ChangeState(State state)
        {
            EnsureInitialized();

            if (state == null) return;
            if (!_allowExternalStates && !_stateRegistry.Values.Contains(state)) { LogError($"Attempted to transition to {state.name} when external states aren't allowed in StateMachine {name}."); return; }
            if (state == CurrentState) return;

            if (CurrentState != null)
            {
                CurrentState.Exiting();
                CurrentState.Exit();
                CurrentState.OnShouldTransition -= ChangeState;
                OnStateExited?.Invoke(CurrentState);
            }

            CurrentState = state;

            CurrentState.Entering();
            CurrentState.Enter();
            CurrentState.OnShouldTransition += ChangeState;
            OnStateEntered?.Invoke(CurrentState);
        }

        protected virtual void Awake()
        {
            int numStates = 0;

            // initialize and register states
            foreach (var state in _states)
            {
                if (_stateRegistry.ContainsKey(state.name)) { LogWarning($"State name {state.name} is conflicting with another state."); continue; }

                _stateRegistry[state.name] = state;
                numStates++;
            }

            Log($"Registered {numStates} states in StateMachine {name}");
        }

        protected virtual void Start()
        {
            if (_initialized) return;

            foreach (var state in StateRegistry.Values)
            {
                state.Initialize(this);
            }

            _initialized = true;

            if (_stateRegistry.Count > 0) ChangeState(_stateRegistry.Values.ToList()[Mathf.Min(StateRegistry.Count - 1, _initialStateIndex)]);
            else LogWarning($"No registered states to change to.");
        }

        protected virtual void Update()
        {
            if (CurrentState == null) return;

            CurrentState.StateUpdate();
        }

        protected virtual void FixedUpdate()
        {
            if (CurrentState == null) return;

            CurrentState.StateFixedUpdate();
        }

        private void EnsureInitialized()
        {
            if (_initialized) return;

            Start();
        }

        #region Debugging
        private void Log(string msg)
        {
            if (!_printWarningsAndErrors) return;

            Debug.Log(msg);
        }

        private void LogWarning(string msg)
        {
            if (!_printWarningsAndErrors) return;

            Debug.LogWarning(msg);
        }

        private void LogError(string msg)
        {
            if (!_printWarningsAndErrors) return;

            Debug.LogError(msg);
        }
        #endregion
    }
}
