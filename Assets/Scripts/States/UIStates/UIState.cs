using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Canvas), typeof(RectTransform))]
public abstract class UIState : MonoBehaviour, IState
{
    public virtual void BeginEnter()
    {

    }

    public virtual void EndEnter()
    {

    }

    public virtual IEnumerable Execute()
    {
        while (true)
        {

            yield return null;
        }
    }

    public virtual event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public virtual void EndExit()
    {

    }
}
