using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class CannonMoveSwipe : MonoBehaviour
{
    [SerializeField] private InputAction position, press;
    [SerializeField] private float swipeThreshhold = 10;
    private Vector2 initialPos;
    private double startPressTime;
    private Vector2 currentPos => position.ReadValue<Vector2>();

    public UnityAction<Vector2> Rotate;
    public UnityAction<float> Fire;

    private void Awake()
    {
        position.Enable();
        press.Enable();
        press.performed += _ => OnStartPress();
        press.canceled += _ => DetectInput();

    }

    private void OnStartPress()
    {
        initialPos = currentPos;
        startPressTime = Time.timeAsDouble;
    }

    private void DetectInput()
    {
        // should account for screen resolution
        Vector2 delta = currentPos - initialPos;

        //for some reason detect swipe is called twice on release catch this bug
        double timeDelta = Time.timeAsDouble - startPressTime;

        if (timeDelta < 0.01f)
        {
            return;
        }

        if (delta.magnitude > swipeThreshhold)
        {
            Debug.LogWarning($"Swiped.. {delta}");
            Rotate?.Invoke(delta);
        }
        else
        {
            Fire?.Invoke((float)timeDelta);
        }

        Debug.LogWarning($"Swipe/Press duration: {timeDelta} s");
    }
}
