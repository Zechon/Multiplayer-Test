using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public int groundHeight = 8;             // flat ground level
    public bool useNoise = false;
    public float noiseScale = 0.05f;

    public int FillChunk(VoxelWorld world, Chunk chunk)
    {
        int cs = world.chunkSize;

        for (int x = 0; x < cs; x++)
            for (int z = 0; z < cs; z++)
            {
                int gx = chunk.chunkCoord.x * cs + x;
                int gz = chunk.chunkCoord.z * cs + z;

                // Determine height of terrain column
                int height = groundHeight;

                if (useNoise)
                {
                    height =
                        Mathf.FloorToInt(Mathf.PerlinNoise(gx * noiseScale, gz * noiseScale) * 10f)
                        + 6;
                }

                // Now fill entire height column
                for (int y = 0; y < cs; y++)
                {
                    int gy = chunk.chunkCoord.y * cs + y;

                    if (gy > height)
                    {
                        chunk.blocks[x, y, z] = 0;           // Air
                    }
                    else if (gy == height)
                    {
                        chunk.blocks[x, y, z] = 3;           // Top block
                    }
                    else if (gy >= height - 3)
                    {
                        chunk.blocks[x, y, z] = 2;           // Middle layer
                    }
                    else
                    {
                        chunk.blocks[x, y, z] = 1;           // Deepest layer
                    }
                }
            }
        return 0;
    }
}
