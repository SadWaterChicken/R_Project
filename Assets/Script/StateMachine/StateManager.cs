using System;
using System.Collections.Generic;
using UnityEngine;



public abstract class StateManager<EState> : MonoBehaviour where EState : Enum
{
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    protected BaseState<EState> CurrentState;
    protected bool IsTransitioningState = false;
    void Start()
    {
        CurrentState.EnterState();
    }
    protected virtual void Update()
    {
        EState nextStateKey = CurrentState.GetNextState();

        if (nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }
        else
        {
            TransitionToState(nextStateKey);
        }
    }

    void TransitionToState(EState stateKey)
    {
        IsTransitioningState = true;
        CurrentState.ExitState();
        CurrentState = States[stateKey];
        CurrentState.EnterState();
        IsTransitioningState = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Handle trigger enter event
        CurrentState.OnTriggerEnter(other);

    }
    void OnTriggerStay(Collider other)
    {
        // Handle trigger stay event
        CurrentState.OnTriggerStay(other);
    }
    void OnTriggerExit(Collider other)
    {
        // Handle trigger exit event
        CurrentState.OnTriggerExit(other);
    }
    void OnCollisionEnter(Collision other)
    {
        // Handle collision enter event
        CurrentState.OnCollisionEnter(other);
    }
    void OnCollisionStay(Collision other)
    {
        // Handle collision stay event
        CurrentState.OnCollisionStay(other);
    }
    void OnCollisionExit(Collision other)
    {
        // Handle collision exit event
        CurrentState.OnCollisionExit(other);
    }
}