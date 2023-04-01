using Unity;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// A state for the main menu
/// </summary>
public class MainMenuState : IState
{

	public void BeginEnter()
	{

	}

	public void EndEnter()
	{
		GoPlayState();
	}

	public IEnumerable Execute()
	{
		while (true)
		{

			yield return null;
		}
	}

	public event EventHandler<StateBeginExitEventArgs> OnBeginExit;

	public void EndExit()
	{
		// Clean up the UI
		//UnityEngine.Object.Destroy(canvas.gameObject);
	}

	private void GoPlayState()
	{
		// Transition to the play state using a screen fade
		var nextState = new PlayState();
		var transition = new ScreenFadeTransition(2);
		var eventArgs = new StateBeginExitEventArgs(nextState, transition);
		OnBeginExit(this, eventArgs);
	}
}