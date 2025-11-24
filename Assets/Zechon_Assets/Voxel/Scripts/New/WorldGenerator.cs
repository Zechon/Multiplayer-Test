using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int groundHeight = 8;             // flat ground level
    public bool useNoise = false;
    public float noiseScale = 0.05f;

    public int FillChunk(VoxelWorld world, Chunk chunk)
    {
        int cs = world.chunkSize;
        int[,] columnHeights = new int[cs, cs];

        for (int x = 0; x < cs; x++)
            for (int z = 0; z < cs; z++)
            {
                int gx = chunk.chunkCoord.x * cs + x;
                int gz = chunk.chunkCoord.z * cs + z;
                int height = groundHeight;

                if (useNoise)
                    height = Mathf.FloorToInt(Mathf.PerlinNoise(gx * noiseScale, gz * noiseScale) * 10f) + 6;

                columnHeights[x, z] = height;
            }

        for (int x = 0; x < cs; x++)
            for (int z = 0; z < cs; z++)
            {
                int height = columnHeights[x, z];

                for (int y = 0; y < cs; y++)
                {
                    int gy = chunk.chunkCoord.y * cs + y;
                    int topSurface = height;

                    // --- Air ---
                    if (gy > topSurface)
                    {
                        chunk.blocks[x, y, z] = 0;
                        continue;
                    }

                    // --- Surface layer ---
                    if (gy == topSurface)
                    {
                        int centerHeight = columnHeights[x, z];

                        // neighbors for slope detection
                        int[] neighbors = new int[]
                        {
                        columnHeights[Mathf.Max(x-1,0), z],
                        columnHeights[Mathf.Min(x+1, cs-1), z],
                        columnHeights[x, Mathf.Max(z-1,0)],
                        columnHeights[x, Mathf.Min(z+1, cs-1)]
                        };

                        bool anyLowerNeighbor = false;
                        bool isValley = true;

                        foreach (int n in neighbors)
                        {
                            if (n < centerHeight) anyLowerNeighbor = true;
                            if (n > centerHeight) isValley = false;
                        }

                        if (anyLowerNeighbor) chunk.blocks[x, y, z] = 5; // Slab edge
                        else if (isValley && Random.value < 0.6f) chunk.blocks[x, y, z] = 6; // Sand
                        else chunk.blocks[x, y, z] = 4; // Full Grass
                    }
                    // --- Upper underground ---
                    else if (gy >= height - 3)
                    {
                        chunk.blocks[x, y, z] = 2; // Dirt
                    }
                    // --- Deeper underground ---
                    else
                    {
                        chunk.blocks[x, y, z] = 1; // Stone
                    }
                }
            }

        for (int x = 0; x < cs; x++)
            for (int z = 0; z < cs; z++)
                for (int y = 0; y < cs; y++)
                {
                    int gx = chunk.chunkCoord.x * cs + x;
                    int gy = chunk.chunkCoord.y * cs + y;
                    int gz = chunk.chunkCoord.z * cs + z;

                    // normalized 3D Perlin noise
                    float caveNoise = Mathf.PerlinNoise(gx * 0.1f, gz * 0.1f) + Mathf.PerlinNoise(gx * 0.1f, gy * 0.1f);
                    if (caveNoise > 1.2f && gy < columnHeights[x, z])
                    {
                        chunk.blocks[x, y, z] = 0; // carve cave

                        // carve entrance near surface
                        if (gy == columnHeights[x, z] - 1) chunk.blocks[x, y + 1, z] = 0;
                    }
                }

        return 0;
    }
}
