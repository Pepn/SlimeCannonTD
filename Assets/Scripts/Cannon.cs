using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cannon : MonoBehaviour
{
    [SerializeField, Required, FoldoutGroup("References")] private GameObject aimTarget;
    [SerializeField, Required, FoldoutGroup("References")] private TowerCombiner towerCombiner;
    [SerializeField, Required, FoldoutGroup("References")] public GameObject towerPrefab;
    [SerializeField, Required, FoldoutGroup("References")] public GameObject inputArea;
    [SerializeField, Required, FoldoutGroup("References")] public Bounds selectionBounds;

    [SerializeField, FoldoutGroup("Settings")] private int combinerInterval;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float speed;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float accuracyRange;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float reloadTime;

    private Vector3 currentAim;
    private bool selectedX, selectedY, selectedZ;
    private World world;
    private Vector3 startPosition;
    private int shootCounter = 0;

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    private Direction currentDirection = Direction.Up;
    private void Start()
    {
        world = GameManager.Instance.World;
        selectionBounds = inputArea.GetComponent<BoxCollider>().bounds;
        startPosition = transform.position;
        ResetSelection();
    }

    private void Update()
    {
        aimTarget.transform.position = currentAim;
        TimeStep();
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!selectedX)
            {
                currentDirection = Direction.Right;
                selectedX = true;
                return;
            }
            else
            {
                selectedY = true;
            }

        }

        if (selectedX && selectedY)
        {
            ShootCannon();
        }
    }

    private void ShootCannon()
    {
        if (shootCounter % combinerInterval == 0)
        {
            //combine
            towerCombiner.TestTowerCombineAtTarget();
        }
        else
        {
            PlaceTower();
        }

        shootCounter++;
        ResetSelection();
    }

    public void PlaceTower()
    {
        Debug.Log($"Placing Tower");
        var bT = TowerManager.Instance.CreateTower(towerPrefab, PlaneHitPoint());
        TowerManager.Instance.AddTower(bT);
    }

    private Vector3 PlaneHitPoint()
    {
        var downDirection = -GameManager.Instance.World.Floor.transform.up;
        Physics.Raycast(transform.position, downDirection, out RaycastHit hit, LayerMask.GetMask("Floor"));

        return transform.position;
    }

    private void ResetSelection()
    {
        currentAim = startPosition;
        selectedX = false;
        selectedY = false;
        selectedZ = false;
        currentDirection = Direction.Up;
    }

    private void TimeStep()
    {

        switch (currentDirection)
        {
            case Direction.Up:
                currentAim.y += speed * Time.deltaTime;
                if (!selectionBounds.Contains(currentAim))
                {
                    currentAim.y = selectionBounds.max.y;
                    currentDirection = Direction.Down;
                }

                break;
            case Direction.Down:
                currentAim.y -= speed * Time.deltaTime;
                if (!selectionBounds.Contains(currentAim))
                {
                    currentAim.y = selectionBounds.min.y;
                    currentDirection = Direction.Up;
                }

                break;
            case Direction.Right:
                currentAim.x += speed * Time.deltaTime;
                if (!selectionBounds.Contains(currentAim))
                {
                    currentAim.x = selectionBounds.max.x;
                    currentDirection = Direction.Left;
                }
                
                break;
            case Direction.Left:
                currentAim.x -= speed * Time.deltaTime;
                if (!selectionBounds.Contains(currentAim))
                {
                    currentAim.x = selectionBounds.min.x;
                    currentDirection = Direction.Right;
                }

                break;
            default:
                Debug.LogError("No Direction, something went wrong");
                break;
        }
    }
}
