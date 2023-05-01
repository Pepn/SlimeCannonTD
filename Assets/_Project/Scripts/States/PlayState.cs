using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// A state for the game play portion of the example
/// </summary>
public class PlayState : UIState
{
	private GameManager gameManager;
    private StateMachine UIStateMachine;
    [SerializeField] private UIIngame uIIngame = default;
    public override void BeginEnter()
	{
		base.BeginEnter();
		//gameManager = UnityEngine.Object.Instantiate(Resources.Load<GameManager>("GameManager"));
		gameManager = FindObjectOfType<GameManager>();

        // Create the state machine
        UIStateMachine = new StateMachine(uIIngame);

        // Run the state machine
        StartCoroutine(UIStateMachine.Execute().GetEnumerator());
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
		Destroy(gameManager);
    }
}