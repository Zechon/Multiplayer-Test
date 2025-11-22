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
                for (int x = 0; x < tileSize; x++)
                    for (int y = 0; y < tileSize; y++)
                        atlas.SetPixel(x + tileSize * i, y + tileSize * face, tex.GetPixel(x, y));
            }

            atlas.Apply();
        }
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
                for (int x = 0; x < tileSize; x++)
                    for (int y = 0; y < tileSize; y++)
                    {
                        normalAtlas.SetPixel(x + i * tileSize, y + face * tileSize, tex.GetPixel(x, y));
                    }
            }
        }
        normalAtlas.Apply();
    }
}
