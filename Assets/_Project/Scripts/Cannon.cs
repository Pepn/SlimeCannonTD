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
    [SerializeField, Required, FoldoutGroup("References")] private Transform cannonHead;
    [SerializeField, Required, FoldoutGroup("References")] private CannonMoveSwipe cannonMoveSwipe;

    [SerializeField, FoldoutGroup("Settings")] private int combinerInterval;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float speed;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float accuracyRange;
    [SerializeField, FoldoutGroup("Settings"), Range(1, 10)] private float reloadTime;
    [SerializeField, FoldoutGroup("Settings"), Range(0, 100)] private float rotationSpeed;
    [SerializeField, FoldoutGroup("Settings"), Range(0, 10)] private float firePowerIncrement;

    private Vector3 currentAim;
    private bool selectedX, selectedY;
    private World world;
    private Vector3 startPosition;
    private int shootCounter = 0;

    private float aimRotation;
    private float _accumulatedFirePower = 0;
    private enum Direction
    {
        Up,
        Down,
        Left,
        Right,
    }

    private enum ShootingState
    {
        Rotating,
        Firing,
    }

    private ShootingState shootingState = ShootingState.Rotating;
    private Direction currentDirection = Direction.Up;
    private void Start()
    {
        world = GameManager.Instance.World;
        selectionBounds = inputArea.GetComponent<BoxCollider>().bounds;
        startPosition = transform.position;
        ResetSelection();
        UpdateTargetDisplay();

        cannonMoveSwipe.Fire += FireCannon;
        cannonMoveSwipe.Rotate += RotateCannon;
    }

    private void Update()
    {
        //aimTarget.transform.position = currentAim;
        //SquaredCannonAimPattern();
        //UpdateCannon();
    }

    private void RotateCannon(Vector2 swipeDelta)
    {
        float rot = swipeDelta.magnitude;

        if (swipeDelta.x < 0)
        {
            rot *= -1;
        }

        Debug.Log($"Rotating Cannon with {rot} degrees..");
        cannonHead.Rotate(new Vector3(0, rot, 0));
    }

    private void FireCannon(float inputTime)
    {
        aimTarget.transform.position = cannonHead.transform.position + (cannonHead.transform.forward * inputTime * firePowerIncrement) * -1;
        ShootCannon();
    }

    private void UpdateCannon()
    {
        if (shootingState == ShootingState.Rotating)
        {
            cannonHead.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
        }

        if (shootingState == ShootingState.Firing)
        {
            _accumulatedFirePower += firePowerIncrement * Time.deltaTime;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            shootingState = ShootingState.Firing;
        }

        if (Keyboard.current.spaceKey.wasReleasedThisFrame)
        {
            ShootCannon();
            _accumulatedFirePower = 0;
            shootingState = ShootingState.Rotating;
        }

        // update aim target
        aimTarget.transform.position = cannonHead.transform.position + (cannonHead.transform.forward * _accumulatedFirePower) * -1;
    }
    private void AlternateCannonFire()
    {
        if (shootCounter % combinerInterval == combinerInterval - 1)
        {
            Debug.Log("Combining Towers..");
            towerCombiner.TestTowerCombineAtTarget();
        }
        else
        {
          PlaceTower();
        }

        UpdateTargetDisplay();
    }
    private void ShootCannon()
    {
        PlaceTower();
        shootCounter++;
        
        levelGrid.IsGridDirty = true;
        ResetSelection();
    }

    // Change cannon shooting to rotate around its origin
    // hold to increase firepower, on start hold stop the rotation (or not!?)
    // the bullets you get have random size (weight) the heavier the more power you need to shoot the same distancet
    // show the next bullet you shoot with its weight, weight is defined by the towertype & upgrade level

    // remove combiner shot into a special slime that does this massive fx and strenght based on the accuracy of the shot ! (dopamine hit)

    // add different bullets the player can choose to add to their deck
    // fire, money, attackspeed, default other cool towers that have bonuses if you shoot them in special arrangements

    // tower cards aka the deck you build either you build the deck during each run (roguelike)
    // OR you build the deck in the main menu (mobile game)

    // meta game tower attachments
    // remove scope -> remove vfx that indicate your shot, gain extra speed/strength/tower strength
    // remove angle -> remove vfx that indicat your shot, gain extra smth
    // increase shooting power
    // increase attack speed
    // increase default size
    // increase health

    private void UpdateTargetDisplay()
    {
        if(shootCounter % combinerInterval == combinerInterval - 1)
        {
            towerCombiner.SetTarget(towerCombiner.GetCombineTarget);
        }
        else
        {
            towerCombiner.SetTarget(towerCombiner.GetTowerTarget);
        }
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


    // makes the cannon move from left to right and then up and down before placements
    private void SquaredCannonAimPattern()
    {
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
