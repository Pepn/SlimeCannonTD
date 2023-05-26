using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening.Core.Easing;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
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
            UpdateGrid(0,0, numCells.x, numCells.y);
            IsGridDirty = false;
        }
    }

    private void InitGrid()
    {
        CreateGridOnObject(FloorPlane);
        cells = new int[numCells.x, numCells.y];
        debugTowers = new bool[numCells.x, numCells.y];
        _boundsExtents = FloorPlane.GetComponent<Renderer>().bounds.extents;
    }

    public Vector2Int PlaneHitPointToGridIndex(Vector3 hitPoint, GameObject floorPlane)
    {
        Vector3 planeSize = floorPlane.GetComponent<Collider>().bounds.size;
        Debug.Log($"planeSize {planeSize}");
        float cellWidth = planeSize.x / numCells.x;
        float cellHeight = planeSize.y / numCells.y;

        // Calculate the offset of the plane's center
        float offsetX = planeSize.x / 2f;
        float offsetY = planeSize.y / 2f;

        // Calculate the grid cell indices
        int cellX = Mathf.FloorToInt((hitPoint.x - floorPlane.transform.position.x + offsetX) / cellWidth);
        int cellY = Mathf.FloorToInt((hitPoint.y - floorPlane.transform.position.y + offsetY) / cellHeight);

        // Ensure the cell indices are within the valid range
        cellX = Mathf.Clamp(cellX, 0, 9);
        cellY = Mathf.Clamp(cellY, 0, 9);

        // The grid cell indices
        int gridCellX = cellX;
        int gridCellY = cellY;

        Debug.Log($"{gridCellX} {gridCellY}");

        return new Vector2Int(gridCellX, gridCellY);
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

    private void UpdateGrid(int x, int y, int width, int height)
    {
        //directly after placing and moving tower the physics object does not move in the same frame, manually pushing physics simulation fixs this
        Physics.autoSimulation = false;
        Physics.Simulate(Time.fixedDeltaTime);
        Physics.autoSimulation = true;

        ClearTowerIdFromGrid();

        Bounds bounds = FloorPlane.GetComponent<Renderer>().bounds;
        for (int i = x; i < x + width; i++)
        {
            for (int j = y; j < y + height; j++)
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
