using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Sirenix.OdinInspector;

public class TowerCombiner : MonoBehaviour
{
    [SerializeField] Texture2D combineTarget;
    [SerializeField] LevelGrid grid;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
                Debug.Log(tTextureResized.GetPixel(x, y).a);
                if(tTextureResized.GetPixel(x,y).a > 0.5)
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

        Debug.Log($"Created bool array: {width}x{height} with {totalTrue} true values");
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
    public void TestTowerCombine(Vector2Int pos, Vector2Int size)
    {
        bool[,] template = TextureToBoolArray(combineTarget, size);
        HashSet<int> towers;
        Debug.Log($"Looking from {pos.x},{pos.y} to {pos.x + template.GetLength(0)}, {pos.y + template.GetLength(1)}");
        int matches = grid.TemplateMatchPosition(template, grid.cells, pos, out towers);
        Debug.Log($"Found {matches} matches.. towers found:{string.Join(", ", towers)}");
    }
}
