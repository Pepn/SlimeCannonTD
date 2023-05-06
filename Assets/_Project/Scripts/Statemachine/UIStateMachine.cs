using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.Android;
using System;
using UnityEditor;
using UnityEditorInternal;

[ExecuteAlways]
public class UIStateMachine : SerializedMonoBehaviour
{
    [field: SerializeField] public State CurrentState => states.TryPeek(out State state) ? state : null;
    [SerializeField, ReadOnly] private Stack<State> states = new Stack<State>();
    [field: SerializeField, ReadOnly] public List<State> PossibleStates = new List<State>();

    [SerializeField, Required, OnValueChanged("OnEnable")] private StateName startState;
    [field: SerializeField] private List<State> StatesStack => states.ToList();
    private void OnEnable()
    {
        states.Clear();
        PossibleStates.Clear();
        PossibleStates.AddRange(FindObjectsOfType<State>().ToList());
        PossibleStates.ForEach(state => state.SetAllObjectsActive(false));
        GetState(startState).SetAllObjectsActive(true);
        SetState(startState);
    }

    public void SetState(State state) => SetState(state.Name);
    public void SetState(string stateName) => SetState((StateName)Enum.Parse(typeof(StateName), stateName));

    [Button]
    public void SetState(StateName newState)
    {
        // Exit the current state
        if (CurrentState != null)
        {
            CurrentState.Exit();
        }

        Debug.Log($"Going to State {newState}");

        // Enter the new state
        states.Push(GetState(newState));
        CurrentState.Enter();
    }

    private State GetState(StateName name) => PossibleStates.Find(s => s.Name == name);

    [Serializable]
    public enum StateName
    {
        InGame = 0,
        InUpgradeScreen = 1,
    }
}