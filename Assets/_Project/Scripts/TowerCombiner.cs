using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

[ExecuteAlways]
public class TowerCombiner : MonoBehaviour
{
    [SerializeField, FoldoutGroup("References")] private Texture2D combineTarget;
    [SerializeField, FoldoutGroup("References")] private LevelGrid grid;
    [SerializeField, FoldoutGroup("References")] private DecalProjector targetProjector;
    [SerializeField, FoldoutGroup("References")] private BoxCollider _targetSpawnArea;

    [SerializeField, FoldoutGroup("Settings")] private List<TargetInfoSO> targets = new List<TargetInfoSO>();


    [SerializeField, FoldoutGroup("Debug"), ReadOnly] private Vector3 centerCombinedTowers;
    [SerializeField, FoldoutGroup("Debug"), ReadOnly] private TargetInfoSO currentTarget;


    private void Awake()
    {

    }

    private void Update()
    {
        //10x10 is size 1
        currentTarget = targets[0];
        targetProjector.size = new Vector3(currentTarget.Size.x * 0.1f, currentTarget.Size.y * 0.1f, targetProjector.size.z);
        targetProjector.material = currentTarget.TargetImage;
        combineTarget = (Texture2D)currentTarget.TargetImage.GetTexture("Base_Map");
    }

    bool[,] TextureToBoolArray(Texture2D targetTexture, Vector2Int size)
    {
        Texture2D tTextureResized = Resize(targetTexture, size.x, size.y);
        int width = tTextureResized.width;
        int height = tTextureResized.height;

        bool[,] template = new bool[width,height];
        int totalTrue = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (tTextureResized.GetPixel(x, y).a > 0.5)
                {
                    template[x, y] = true;
                    totalTrue++;
                }
                else
                {
                    template[x, y] = false;
                }
            }
        }

        //Debug.Log($"Created bool array: {width}x{height} with {totalTrue} true values");
        return template;
    }

    private static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Point;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }

    [Button]
    public void TestTowerCombine(Vector2Int pos)
    {
        bool[,] template = TextureToBoolArray(combineTarget, currentTarget.Size);
        Debug.Log($"Looking from {pos.x},{pos.y} to {pos.x + template.GetLength(0)}, {pos.y + template.GetLength(1)}");
        TargetHitInfo hitInfo = grid.TemplateMatchPosition(template, grid.cells, pos);
        Debug.Log(hitInfo.ToString());


        StartCoroutine(TowerCombineTransformTowers(hitInfo));
    }

    private float CalcNewTowerWeight(TargetHitInfo hitInfo) => 
        Mathf.Max(hitInfo.HitTowers.Sum(t => t.Weight) * hitInfo.HitAccuracy, MinNewTowerWeight(hitInfo));

    private float MinNewTowerWeight(TargetHitInfo hitInfo) => (hitInfo.HitTowers.Count * hitInfo.HitAccuracy) + 1;

    private IEnumerator TowerCombineTransformTowers(TargetHitInfo hitInfo)
    {
        if (hitInfo.HitTowers != null && hitInfo.HitTowers.Count != 0)
        {
            // calculate center
            centerCombinedTowers = CalculateCenterPosition(hitInfo.HitTowers);
        }
        else
        {
            Debug.LogWarning($"HitInfo is Null");
        }

        // move towers towards each other
        yield return StartCoroutine(MoveObjectsTowardsEachOther(hitInfo.HitTowers, centerCombinedTowers, 0.1f));

        // merge meshes to one single mesh ?
        TowerManager.Instance.CreateTower(currentTarget.TowerPrefab, centerCombinedTowers, CalcNewTowerWeight(hitInfo));

        // remove towers
        for (int i = 0; i < hitInfo.HitTowers.Count; i++)
        {
            if (hitInfo.HitTowers[i] == null)
            {
                Debug.LogWarning("For  some reason this tower is null, skipping..");
                continue;
            }

            Debug.Log($"Deleting tower..");
            TowerManager.Instance.RemoveTower(hitInfo.HitTowers[i]);
        }

        // recolor mesh to show upgrade version

        // spawn new combined tower

        yield return null;
    }

    /// <summary>
    /// Coroutine to move the objects towards each other until they collide.
    /// </summary>
    private IEnumerator MoveObjectsTowardsEachOther(List<BaseTower> towers, Vector3 center, float mergeDuration)
    {
        List<bool> collidedTowers = Enumerable.Range(0, towers.Count).Select(i => false).ToList();
        float t = 0.0f;
        while (!collidedTowers.Exists(t => false) && t <= mergeDuration)
        {
            t += Time.deltaTime;

            // Move the objects towards the center position
            for (int i = 0; i < towers.Count; i++)
            {
                if (!towers[i])
                {
                    continue;
                }

                if (!collidedTowers[i])
                {
                    towers[i].transform.position = Vector3.Lerp(towers[i].transform.position, center, t);
                }
            }

            // Check for collisions between the objects
            for (int i = 0; i < towers.Count; i++)
            {
                for (int j = i + 1; j < towers.Count; j++)
                {
                    if (towers[i] == null || towers[j] == null)
                    {
                        continue;
                    }

                    if (towers[i].GetComponent<Collider>().bounds.Intersects(towers[j].GetComponent<Collider>().bounds))
                    {
                        collidedTowers[i] = true;
                        collidedTowers[j] = true;
                    }
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Calculate and return the center position.
    /// </summary>
    /// <param name="towers">List of towers to get the centre for.</param>
    /// <returns>The center position.</returns>
    public Vector3 CalculateCenterPosition(List<BaseTower> towers)
    {
        Vector3 centerPosition = Vector2.zero;
        float totalWeight = 0f;

        for (int i = 0; i < towers.Count; i++)
        {
            if (towers[i] == null)
            {
                continue;
            }

            centerPosition += towers[i].transform.position * towers[i].Weight;
            totalWeight += towers[i].Weight;
        }

        if (totalWeight > 0)
        {
            centerPosition /= totalWeight;
        }

        return centerPosition;
    }

    [Button]
    public void TestTowerCombineAtTarget()
    {
        TestTowerCombine(TargetToGridTransform());
    }

    private Vector2Int TargetToGridTransform()
    {
        Vector2 gridPosP = new Vector2(
            (transform.localPosition.x - _targetSpawnArea.bounds.min.x) / (_targetSpawnArea.bounds.max.x - _targetSpawnArea.bounds.min.x),
            (transform.localPosition.y - _targetSpawnArea.bounds.min.y) / (_targetSpawnArea.bounds.max.y - _targetSpawnArea.bounds.min.y));
        Vector2Int gridPosInSquares = new Vector2Int((int)(gridPosP.x * grid.numCells.x), (int)(gridPosP.y * grid.numCells.y));

        // move half the size of the target
        gridPosInSquares -= new Vector2Int(currentTarget.Size.x / 2, currentTarget.Size.y / 2);
        return gridPosInSquares;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(centerCombinedTowers, 0.1f);
    }
}
