using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurrentSpawner : MonoBehaviour
{
    private Vector3 currentAim;
    private bool selectedX, selectedY, selectedZ;
    private World world;
    [SerializeField] private GameObject aimTarget;
    private Bounds selectionBounds;
    private Vector3 startPosition;

    public GameObject towerPrefab;
    public GameObject inputArea;
    [Header("Settings")]
    [SerializeField, Range(1, 10)] private float speed;
    [SerializeField, Range(1, 10)] private float accuracyRange;
    [SerializeField, Range(1, 10)] private float reloadTime;
    [SerializeField] private bool visible;
    private enum Direction
    {
        Up, Down, Left, Right
    }
    private  Direction currentDirection = Direction.Up;
    // Start is called before the first frame update

    void Start()
    {
        world = GameManager.Instance.World;
        selectionBounds = inputArea.GetComponent<BoxCollider>().bounds;
        startPosition = transform.position;
        ResetSelection();
    }

    // Update is called once per frame
    void Update()
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
            PlaceTower();
            ResetSelection();
        }

    }

    public void PlaceTower()
    {
        var bT = TowerManager.Instance.CreateTower(towerPrefab, PlaneHitPoint());
        TowerManager.Instance.AddTower(bT);
    }

    private Vector3 PlaneHitPoint()
    {
        var downDirection = -GameManager.Instance.World.Floor.transform.up;
        Physics.Raycast(transform.position, downDirection, out RaycastHit hit, LayerMask.GetMask("Floor"));
        return hit.point;
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
                currentAim.z += speed * Time.deltaTime;
                if (!selectionBounds.Contains(currentAim))
                {
                    currentAim.z = selectionBounds.max.z;
                    currentDirection = Direction.Down;
                }
                break;
            case Direction.Down:
                currentAim.z -= speed * Time.deltaTime;
                if (!selectionBounds.Contains(currentAim))
                {
                    currentAim.z = selectionBounds.min.z;
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
