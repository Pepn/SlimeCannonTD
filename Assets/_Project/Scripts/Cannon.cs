using Sirenix.OdinInspector;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cannon : MonoBehaviour
{
    [SerializeField, Required, FoldoutGroup("References")] private GameObject towerPrefab;
    [SerializeField, Required, FoldoutGroup("References")] private GameObject inputArea;
    [SerializeField, Required, FoldoutGroup("References")] private Bounds selectionBounds;
    [SerializeField, Required, FoldoutGroup("References")] private GameObject aimTarget;
    [SerializeField, Required, FoldoutGroup("References")] private TowerCombiner towerCombiner;
    [SerializeField, Required, FoldoutGroup("References")] private LevelGrid levelGrid;

    [SerializeField, FoldoutGroup("Settings")] private int combinerInterval;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float speed;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float accuracyRange;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float reloadTime;

    private Vector3 currentAim;
    private bool selectedX, selectedY;
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
            if (!selectedX && (currentDirection == Direction.Left || currentDirection == Direction.Right))
            {
                currentDirection = RandomUpOrDown;
                selectedX = true;
                return;
            }

            if (!selectedY && (currentDirection == Direction.Up || currentDirection == Direction.Down))
            {
                currentDirection = RandomLeftOrRight;
                selectedY = true;
                return;
            }
        }

        if (selectedX && selectedY)
        {
            ShootCannon();
        }
    }

    private void ShootCannon()
    {
        shootCounter++;
        if (shootCounter % combinerInterval == 0)
        {
            Debug.Log("Combining Towers..");
            towerCombiner.TestTowerCombineAtTarget();
        }
        else
        {
            PlaceTower();
        }

        levelGrid.IsGridDirty = true;
        ResetSelection();
    }

    private void PlaceTower()
    {
        Vector3 targetPoint = PlaneHitPoint();
        TowerManager.Instance.CreateTower(towerPrefab, targetPoint, 1.0f);
        Vector2Int targetedGridCellCenter = levelGrid.PlaneHitPointToGridIndex(targetPoint, levelGrid.FloorPlane);
    }

    private Vector3 PlaneHitPoint()
    {
        Vector3 downDirection = -levelGrid.FloorPlane.transform.up;
        Physics.Raycast(transform.position + (levelGrid.FloorPlane.transform.up * 5), downDirection, out RaycastHit hit, 100, LayerMask.GetMask("Floor"));
        return hit.point;
    }

    private void ResetSelection()
    {
        currentAim = startPosition;
        selectedX = false;
        selectedY = false;
        currentDirection = RandomDirection;
    }

    private Direction RandomDirection => (Direction)UnityEngine.Random.Range(0, 4);
    private Direction RandomUpOrDown => (Direction)UnityEngine.Random.Range(0, 2);
    private Direction RandomLeftOrRight => (Direction)UnityEngine.Random.Range(2, 4);


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 downDirection = -levelGrid.FloorPlane.transform.up;
        Gizmos.DrawRay(transform.position + (levelGrid.FloorPlane.transform.up * 5), downDirection*100);
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
