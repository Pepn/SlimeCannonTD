using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

public class State : MonoBehaviour
{
    [field: SerializeField] public UIStateMachine.StateName Name { get; private set; }
    [field: SerializeField, ChildGameObjectsOnly] public List<GameObject> Objects { get; private set; } = new ();
    [SerializeField] UnityEvent OnEnter, OnExit;
    public virtual void Enter()
    {
        Debug.Log($"Entering {gameObject.name}");
        OnEnter?.Invoke();
    }
    public virtual void Update() { }
    public virtual void Exit()
    {
        OnExit?.Invoke();
    }

    public void SetAllObjectsActive(bool on) => Objects.ForEach(obj => obj.SetActive(on));
}