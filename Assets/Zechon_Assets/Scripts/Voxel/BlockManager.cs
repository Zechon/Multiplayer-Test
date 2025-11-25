using UnityEngine;

public class BlockManager : MonoBehaviour
{
    public BlockClass[] allBlocks;
    public Texture2D atlas;
    public int tileSize = 32;

    [ContextMenu("Build Atlas")]
    public void BuildAtlas()
    {
        int numBlocks = allBlocks.Length;
        atlas = new Texture2D(tileSize * numBlocks, tileSize * 3);
        atlas.filterMode = FilterMode.Point;

        for (int i = 0; i < numBlocks; i++)
        {
            BlockClass b = allBlocks[i];
            if (b == null) continue;

            for (int face = 0; face < 3; face++)
            {
                Texture2D tex = b.blockFaceTextures[face];
                int texHeight = b.isSlab && face == 0 ? tileSize / 2 : tileSize; // sides half height for slabs
                for (int x = 0; x < tileSize; x++)
                    for (int y = 0; y < texHeight; y++)
                        atlas.SetPixel(x + tileSize * i, y + tileSize * face, tex.GetPixel(x, y));
            }

        }

        atlas.Apply();
    }

    public Texture2D normalAtlas;

    [ContextMenu("Build Normal Atlas")]
    public void BuildNormalAtlas()
    {
        int numBlocks = allBlocks.Length;

        normalAtlas = new Texture2D(tileSize * numBlocks, tileSize * 3);
        normalAtlas.filterMode = FilterMode.Bilinear;

        for (int i = 0; i < numBlocks; i++)
        {
            BlockClass b = allBlocks[i];

            for (int face = 0; face < 3; face++)
            {
                Texture2D tex = b.blockFaceNormalMaps[face];
                int texHeight = b.isSlab && face == 0 ? tileSize / 2 : tileSize;
                for (int x = 0; x < tileSize; x++)
                    for (int y = 0; y < texHeight; y++)
                        normalAtlas.SetPixel(x + tileSize * i, y + tileSize * face, tex.GetPixel(x, y));
            }
        }
        normalAtlas.Apply();
    }
}
