using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StartGame : MonoBehaviour
{
    [SerializeField] private PlayState playState = default;
	void Start()
	{
		// Create the state machine
		var stateMachine = new StateMachine(playState);
		
		// Run the state machine
		StartCoroutine(stateMachine.Execute().GetEnumerator());
	}
}
