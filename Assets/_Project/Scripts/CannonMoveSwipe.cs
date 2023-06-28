using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

public class CannonMoveSwipe : MonoBehaviour
{
    [SerializeField] private InputAction position, press, pressHold;
    [SerializeField] private float swipeThreshhold = 25;
    [SerializeField] private float minFirePowerThreshhold = 0.2f;
    [SerializeField] private float shootAndRotateTimeThreshhold = 0.5f;

    private Vector2 initialPos;
    private float startPressTime;
    private Vector2 currentPos => position.ReadValue<Vector2>();

    public UnityAction<Vector2> Rotate;
    public UnityAction Fire;
    public UnityAction StartFiring;
    public UnityAction StartRotating;
    public UnityAction<float> FirePower;
    [SerializeField, ReadOnly] private bool isPressing = false;
    [SerializeField, ReadOnly] private bool isSwiping = false;
    [SerializeField, ReadOnly] private bool isShooting = false;
    [SerializeField, ReadOnly] private Vector2 inputDelta;
    [SerializeField, ReadOnly] private float timeDelta;

    private void Awake()
    {
        position.Enable();
        press.Enable();
        pressHold.Enable();
        press.started += _ => OnStartPress();
        press.canceled += _ => ReleaseInput();
    }

    private void Update()
    {
        if (isPressing)
        {
            UpdateFirePower();

            if (isShooting)
            {
                return;
            }

            // should account for screen resolution
            inputDelta = currentPos - initialPos;

            if (inputDelta.magnitude > swipeThreshhold)
            {
                StartRotating?.Invoke();
                isSwiping = true;
            }
        }
    }

    private void UpdateFirePower()
    {
        //for some reason detect swipe is called twice on release catch this bug
        timeDelta = Time.time - startPressTime;

        //delay updating the start firing visuals
        if (timeDelta > shootAndRotateTimeThreshhold)
        {
            isShooting = true;
            StartFiring?.Invoke();
        }

        FirePower.Invoke((float)timeDelta);
    }

    private void OnStartPress()
    {
        Debug.LogWarning("ON START PRESS");
        initialPos = currentPos;
        startPressTime = Time.time;

        isPressing = true;
    }

    private void ReleaseInput()
    {
        if (isSwiping)
        {
            Rotate?.Invoke(inputDelta);
        }
        else
        {
            if (timeDelta > minFirePowerThreshhold)
            {
                Fire?.Invoke();
            }
        }

        isPressing = false;
        isSwiping = false;
        inputDelta = Vector2.zero;
        isShooting = false;
    }
}
