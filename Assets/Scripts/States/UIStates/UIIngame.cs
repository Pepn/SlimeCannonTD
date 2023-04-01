using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class UIIngame : UIState
{
    GameManager gameManager;
    public override void BeginEnter()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public override void EndEnter()
    {

    }

    public override IEnumerable Execute()
    {
        while (true)
        {
            yield return null;
        }
    }

    public override event EventHandler<StateBeginExitEventArgs> OnBeginExit;

    public override void EndExit()
    {

    }
}
