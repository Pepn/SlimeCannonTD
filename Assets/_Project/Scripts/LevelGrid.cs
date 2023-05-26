using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core.Easing;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;

public class LevelGrid : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Debug"), ReadOnly] private Vector3 cellSize;
    [SerializeField, FoldoutGroup("Debug"), ReadOnly] private Vector3 gridSize;
    [SerializeField, OnValueChanged("InitGrid")] public Vector3Int numCells;
    [SerializeField] public int[,] cells;
    [SerializeField] public bool[,] debugTowers;
    [field: SerializeField] public GameObject FloorPlane { get; private set; }

    public Action OnGridChanged;
    private List<int> towersReadyForCleanup = new List<int>();
    private Vector3 _boundsExtents;

    /// <summary>
    /// Whether the LevelGrid conatiner should be updated due to some change in the level.
    /// </summary>
    public bool IsGridDirty { get; set; }

    private void OnEnable()
    {
        InitGrid();
        TowerManager.Instance.OnRemoveTower += AddTowerIdsToClear;
    }

    private void OnDisable()
    {
        TowerManager.Instance.OnRemoveTower -= AddTowerIdsToClear;
    }

    private void Update()
    {
        if (IsGridDirty)
        {
            //UpdateGrid();
            IsGridDirty = false;
        }

        int objectCount = 1023;

        // Create temporary arrays for the raycast info. Lets use the TempJob allocator,
        // this means we have to dispose them when the job has finished - its short lived data
        var raycastCommands = new NativeArray<RaycastCommand>(objectCount, Allocator.TempJob);
        var raycastHits = new NativeArray<RaycastHit>(objectCount, Allocator.TempJob);

        // Lets schedule jobs to do a collision raycast for each object. One job Prepare    s all the raycast commands,
        // the second actually does the raycasts.
        var setupRaycastsJob = new PrepareRaycasts
        {
            Raycasts = raycastCommands,
            Grid = GetGrid(cells),
            cellSize = cellSize,
            boundsExtents = FloorPlane.GetComponent<Renderer>().bounds.extents,
        };

        var setupDependency = setupRaycastsJob.Schedule(objectCount, 32);

        var raycastDependency = RaycastCommand.ScheduleBatch(raycastCommands, raycastHits, 32, setupDependency);

        var UpdateGridJob = new UpdateGridParralel
        {
            Grid = GetGrid(cells),
            Hits = raycastHits,
        };

        var updateGridDependency = UpdateGridJob.Schedule(objectCount, 32, raycastDependency);


        // All the jobs we want to execute have been scheduled! By calling .Complete() on the last job in the
        // chain, Unity makes the main thread help out with scheduled jobs untill they are all complete. 
        // then we can move on and use the data caluclated in the jobs safely, without worry about data being changed
        // by other threads as we try to use it - we *know* all the work is done
        //https://github.com/LotteMakesStuff/SimplePhysicsDemo/blob/master/Assets/SimpleJobifiedPhysics.cs
        updateGridDependency.Complete();

        //dispose temp arrays
        raycastCommands.Dispose();
        raycastHits.Dispose();
    }

    private void InitGrid()
    {
        CreateGridOnObject(FloorPlane);
        cells = new int[numCells.x, numCells.y];
        debugTowers = new bool[numCells.x, numCells.y];
        _boundsExtents = FloorPlane.GetComponent<Renderer>().bounds.extents;
    }

    private void CreateGridOnObject(GameObject obj)
    {
        Bounds bounds = obj.GetComponent<Renderer>().bounds;
        gridSize = new Vector3(bounds.size.x, bounds.size.y, 0);
        cellSize = new Vector3(gridSize.x / numCells.x, gridSize.y / numCells.y, 0);
    }

    private void AddTowerIdsToClear(BaseTower tower)
    {
        towersReadyForCleanup.Add(tower.Id);
    }

    /// <summary>
    /// After deletion or upgrade make sure to reset all the grid cells to 0.
    /// </summary>
    /// <param name="id">The tower Id to clear.</param>
    private void ClearTowerIdFromGrid()
    {
        for (int i = 0; i < numCells.x; i++)
        {
            for (int j = 0; j < numCells.y; j++)
            {
                foreach (int id in towersReadyForCleanup)
                {
                    if (cells[i, j] == id)
                    {
                        cells[i, j] = 0;
                    }
                }
            }
        }
    }

    public struct PrepareRaycasts : IJobParallelFor {

        public NativeArray<int> Grid;
        [Unity.Collections.ReadOnly] public Vector3 cellSize;
        [Unity.Collections.ReadOnly] public Vector3 boundsExtents;
        public NativeArray<RaycastCommand> Raycasts;

        public void Execute(int index)
        {
            Grid[index] = index;

            Raycasts[index] = new RaycastCommand(new Vector3(index / cellSize.x, index % cellSize.y, -5.0f) - boundsExtents + (cellSize * 0.5f), new Vector3(0, 0, 10), QueryParameters.Default);
        }
    }

    // Integrate the velocity into all the objects positions. We use the
    // Raycast hit data to decide how much of the velocity to integrate -
    // we dont want to tunnel though the colliders in the scene!
    struct UpdateGridParralel : IJobParallelFor
    {

        public NativeArray<int> Grid;
        [ReadOnly]
        public NativeArray<RaycastHit> Hits;

        public void Execute(int index)
        {
            if (Hits[index].normal != Vector3.zero)//.collider.tag == "Tower")
            {
                if (Grid[index] == 0)
                {
                    Grid[index] = Hits[index].collider.gameObject.GetComponent<BasicTower>().Id;
                }
                else //its impossible to have a single value array value store multiple towers so just pick one of the towers at random
                {
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        Grid[index] = Hits[index].collider.gameObject.GetComponent<BasicTower>().Id;
                    }
                }
            }
            else
            {
                Grid[index] = 0;
            }
        }
    }

    public NativeArray<int> GetGrid(int[,] cells)
    {
        Debug.Log($"Gettin grid from cells {cells.GetLength(0)}");
        int index = 0;
        NativeArray<int> grid = new NativeArray<int>(cells.GetLength(0) * cells.GetLength(1), Allocator.Persistent);
        for (int i = 0; i < cells.GetLength(0); i++)
        {
            for (int j = 0; j < cells.GetLength(1); j++)
            {
                grid[index] = cells[i, j]; // Assign values from 2D to 1D array
                index++;
            }
        }

        return grid;
    }

    private void UpdateGrid()
    {
        //directly after placing and moving tower the physics object does not move in the same frame, manually pushing physics simulation fixs this
        Physics.autoSimulation = false;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.autoSimulation = true;

        ClearTowerIdFromGrid();

        Bounds bounds = FloorPlane.GetComponent<Renderer>().bounds;
        for (int i = 0; i < numCells.x; i++)
        {
            for (int j = 0; j < numCells.y; j++)
            {
                Ray ray = new Ray(new Vector3(i*cellSize.x, j*cellSize.y, -5.0f) - bounds.extents + cellSize/2, new Vector3(0, 0, 10));
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo) && hitInfo.collider.tag == "Tower")
                {
                    if (cells[i, j] == 0)
                    {
                        Debug.DrawRay(ray.origin, ray.direction * 100);
                        cells[i, j] = hitInfo.collider.gameObject.GetComponent<BasicTower>().Id;
                    }
                    else //its impossible to have a single value array value store multiple towers so just pick one of the towers at random
                    {
                        if (UnityEngine.Random.value > 0.5f)
                        {
                            cells[i, j] = hitInfo.collider.gameObject.GetComponent<BasicTower>().Id;
                        }
                    }
                }
                else
                {
                    cells[i, j] = 0;
                }
            }
        }
    }

    public void TemplateMatch(bool[,] template, int[,] searchSpace, out Vector2Int pos, out int mostMatches)
    {
        mostMatches = 0;
        pos = new Vector2Int(0, 0);
        for (int y = 0; y < searchSpace.GetLength(1) - template.GetLength(1); y++)
        {
            for (int x = 0; x < searchSpace.GetLength(0) - template.GetLength(0); x++)
            {
                int matches = 0;
                //loop through template image
                for (int j = 0; j < template.GetLength(1); j++)
                {
                    for (int i = 0; i < template.GetLength(0); i++)
                    {
                        //Debug.Log($"{x + i} {y + j}");
                        int searchPixel = cells[x + i,y + j];
                        bool templatePixel = template[i, j];

                        if (searchPixel != 0 && templatePixel == true)
                            matches++;
                    }
                }

                if(matches > mostMatches)
                {
                    mostMatches = matches;
                    pos = new Vector2Int(x, y);
                }
            }
        }
    }

    private void ClearArray(bool[,] array)
    {
        for (int j = 0; j < numCells.x; j++)
        {
            for (int i = 0; i < numCells.y; i++)
            {
                debugTowers[j, i] = false;
            }
        }
    }

    public TargetHitInfo TemplateMatchPosition(bool[,] template, int[,] searchSpace, Vector2Int pos)
    {
        int matches = 0;
        List<BaseTower> towers = new List<BaseTower>();

        ClearArray(debugTowers);

        //loop through template image
        //Debug.Log($"Position given {pos.x}, {pos.y}");
        for (int j = 0; j < template.GetLength(1); j++)
        {
            for (int i = 0; i < template.GetLength(0); i++)
            {
                int gridX = pos.x + i;
                int gridY = pos.y + j;
                if (gridX >= searchSpace.GetLength(0) || gridY >= searchSpace.GetLength(1) || gridX < 0 || gridY < 0)
                {
                    continue;
                }

                //get the pixel value, each value expresses the tower ID
                int searchPixel = 0;
                try
                {
                    searchPixel = cells[pos.x + i, pos.y + j];
                }
                catch
                {
                    Debug.LogError($"Out of bounds {pos.x + i} {pos.y + j}");
                }

                bool templatePixel = template[i, j];

                if (searchPixel != 0 && templatePixel)
                {
                    if (TowerManager.Instance.GetTower(searchPixel) == null)
                    {
                        Debug.LogWarning("Get Tower is NULL");
                    }
                    else
                    {
                        matches++;
                        towers.Add(TowerManager.Instance.GetTower(searchPixel));
                    }
                }

                if (templatePixel)
                {
                    debugTowers[pos.x + i, pos.y + j] = true;
                }
                else
                {
                    debugTowers[pos.x + i, pos.y + j] = false;
                }
            }
        }

        TargetHitInfo hitInfo = new TargetHitInfo(TotalCellsTargeted(), matches, towers.Distinct().ToList());
        //Debug.Log($"Looking from {pos.x},{pos.y} to {pos.x + template.GetLength(0)}, {pos.y + template.GetLength(1)} searching in {TotalCellsTargeted()} cells." );
        return hitInfo;
    }

    private int TotalCellsTargeted()
    {
        int sumPixels = 0;
        for (int j = 0; j < debugTowers.GetLength(1); j++)
        {
            for (int i = 0; i < debugTowers.GetLength(0); i++)
            {
                if (debugTowers[i, j])
                {
                    sumPixels++;
                }
            }
        }

        return sumPixels;
    }

    private void OnDrawGizmosSelected()
    {
        // Draws a blue line from this transform to the target
        Gizmos.color = Color.blue;
        Bounds bounds = FloorPlane.GetComponent<Renderer>().bounds;
        for (int i = 0; i < numCells.x; ++i)
        {
            Gizmos.DrawLine(new Vector3(i*cellSize.x, 0, 0) - bounds.extents, new Vector3(i*cellSize.x, gridSize.y, 0) - bounds.extents);
        }

        for (int j = 0; j < numCells.y; j++)
        {
            Gizmos.DrawLine(new Vector3(gridSize.x, j*cellSize.y, 0) - bounds.extents, new Vector3(0, j*cellSize.y, 0) - bounds.extents);
        }

        for (int i = 0; i < numCells.x; i++)
        {
            for (int j = 0; j < numCells.y; j++)
            {
                if (cells[i, j] != 0)
                {
                    Vector3 cellPos = (new Vector3(i, j, 0) * cellSize.x) - bounds.extents + (cellSize * 0.5f);
                    Handles.Label(cellPos, cells[i,j].ToString());
                }
            }
        }

        Gizmos.color = Color.red;
        for (int i = 0; i < numCells.x; i++)
        {
            for (int j = 0; j < numCells.y; j++)
            {
                if (debugTowers[i, j])
                {
                    Gizmos.DrawSphere((new Vector3(i, j, 0) * cellSize.x) - bounds.extents + (cellSize / 2), 0.1f);
                }
            }
        }
    }
}

/// <summary>
/// Information container for the target hits.
/// </summary>
public class TargetHitInfo
{
    /// <summary>
    /// Gets the Total Cells that the target has active.
    /// </summary>
    public int TotalCellsTargeted { get; private set; }

    /// <summary>
    /// Gets the Total number of towers that were in the active target.
    /// </summary>
    public int TotalCellsHit { get; private set; }

    /// <summary>
    /// Gets The list of towers that are included in this target hit.
    /// </summary>
    public List<BaseTower> HitTowers { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TargetHitInfo"/> class.
    /// </summary>
    /// <param name="totalCellsTargeted">Total Cells that the target has active.</param>
    /// <param name="totalCellsHit">Total Cells that contained a tower.</param>
    /// <param name="towers">List of tower IDs that was found in the target hit.</param>
    public TargetHitInfo(int totalCellsTargeted, int totalCellsHit, List<BaseTower> towers)
    {
        TotalCellsTargeted = totalCellsTargeted;
        TotalCellsHit = totalCellsHit;
        HitTowers = towers;
        Debug.Log($"Concat list {HitTowers.Count}");
    }

    /// <inheritdoc/>
    public override string ToString() => $"(Target Hit Info: {TotalCellsHit}/{TotalCellsTargeted}, Accuracy: {HitAccuracy.ToString("0.00")}, TowersHit: {HitTowers.Count}";

    /// <summary>
    /// Gets the hit accuracy.
    /// </summary>
    public float HitAccuracy => (float)TotalCellsHit / (float)TotalCellsTargeted;
}
