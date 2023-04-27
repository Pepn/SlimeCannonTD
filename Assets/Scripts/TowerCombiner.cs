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

    [SerializeField] private Vector2Int _size;
    [SerializeField, FoldoutGroup("References")] private DecalProjector targetProjector;
    [SerializeField, FoldoutGroup("References")] private BoxCollider _targetSpawnArea;

    private void Awake()
    {

    }

    private void Update()
    {
        //10x10 is size 1
        targetProjector.size = new Vector3(_size.x * 0.1f, _size.y * 0.1f, 1);
        combineTarget = (Texture2D)targetProjector.material.GetTexture("Base_Map");
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
        bool[,] template = TextureToBoolArray(combineTarget, _size);
        Debug.Log($"Looking from {pos.x},{pos.y} to {pos.x + template.GetLength(0)}, {pos.y + template.GetLength(1)}");
        TargetHitInfo hitInfo = grid.TemplateMatchPosition(template, grid.cells, pos);
        Debug.Log(hitInfo.ToString());

        TowerCombineTransformTowers(hitInfo);
    }

    private void TowerCombineTransformTowers(TargetHitInfo hitInfo)
    {
        for (int i = 0; i < hitInfo.HitTowers.Count; i++)
        {
            TowerManager.Instance.RemoveTower(hitInfo.HitTowers[i]);
        }
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
        gridPosInSquares -= new Vector2Int(_size.x / 2, _size.y / 2);
        return gridPosInSquares;
    }
}
