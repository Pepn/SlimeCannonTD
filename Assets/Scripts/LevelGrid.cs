using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;

[ExecuteAlways]
public class LevelGrid : MonoBehaviour
{
    [SerializeField, FoldoutGroup("Debug"), ReadOnly] private Vector3 cellSize;
    [SerializeField, FoldoutGroup("Debug"), ReadOnly] private Vector3 gridSize;
    [SerializeField, OnValueChanged("InitGrid")] private Vector3Int numCells;
    [SerializeField] public int[,] cells;
    [SerializeField] public bool[,] debugTowers;
    [SerializeField] private GameObject floorPlane;

    void OnEnable()
    {
        InitGrid();
    }

    void InitGrid()
    {
        CreateGridOnObject(floorPlane);
        cells = new int[numCells.x, numCells.y];
        debugTowers = new bool[numCells.x, numCells.y];
    }

    void CreateGridOnObject(GameObject obj)
    {
        print("creategrid");
        Bounds bounds = obj.GetComponent<Renderer>().bounds;
        gridSize = new Vector3(bounds.size.x, bounds.size.y, 0);
        cellSize = new Vector3(gridSize.x / numCells.x, gridSize.y / numCells.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateGrid();
        if (Keyboard.current[Key.Space].wasPressedThisFrame)
        {
            TestTemplateMatch();
        }
    }

    void TestTemplateMatch()
    {
        bool[,] temp = new bool[,] { { true, true, true }, { true, true, true }, { true, true, true } };

        Vector2Int pos;
        int matches;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        TemplateMatch(temp, cells, out pos, out matches);
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        Debug.Log($"Best Pos {pos} with matches {matches} found in {elapsedMs}ms");
    }


    public void UpdateGrid()
    {
        Bounds bounds = floorPlane.GetComponent<Renderer>().bounds;
        for (int i = 0; i < numCells.x; i++)
        {
            for (int j = 0; j < numCells.y; j++)
            {
                Ray ray = new Ray(new Vector3(i*cellSize.x, j*cellSize.y, 1) - bounds.extents + cellSize/2, new Vector3(0, 0, -1));
                //Debug.DrawRay(ray.origin, ray.direction, Color.red, 3.0f);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, 10) && hitInfo.collider.tag == "Tower")
                {
                    cells[i, j] = hitInfo.collider.gameObject.GetComponent<BasicTower>().Id;
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

    public int TemplateMatchPosition(bool[,] template, int[,] searchSpace, Vector2Int pos, out HashSet<int> towers)
    {
        int matches = 0;
        towers = new HashSet<int>();
        //loop through template image
        Debug.Log($"Position given {pos.x}, {pos.y}");
        for (int j = 0; j < template.GetLength(1); j++)
        {
            for (int i = 0; i < template.GetLength(0); i++)
            {
                int gridX = pos.x + i;
                int gridY = pos.y + j;
                if (gridX >= searchSpace.GetLength(0) || gridY >= searchSpace.GetLength(1) || gridX < 0 || gridY < 0)
                {
                    Debug.Log("Looking outside the grid skipping..");
                    continue;
                }

                //Debug.Log($"{x + i} {y + j}");
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

                if (searchPixel != 0 && templatePixel == true)
                {
                    towers.Add(searchPixel);
                    matches++;
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
        Debug.Log($"Looking from {pos.x},{pos.y} to {pos.x+ template.GetLength(0)}, {pos.y + template.GetLength(1)}" );
        return matches;
    }

    private void OnDrawGizmos()
    {
        // Draws a blue line from this transform to the target
        Gizmos.color = Color.blue;
        Bounds bounds = floorPlane.GetComponent<Renderer>().bounds;
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
                if (cells[i,j] != 0)
                {
                    Gizmos.DrawSphere((new Vector3(i, j, 0)*cellSize.x) - bounds.extents + (cellSize*0.5f), 0.1f);
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
                    Gizmos.DrawSphere(new Vector3(i, j, 0) * cellSize.x - bounds.extents + cellSize / 2, 0.1f);
                }
            }
        }
    }
}