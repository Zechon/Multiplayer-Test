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
            for (int y = 0; y < cs; y++)
                for (int z = 0; z < cs; z++)
                {
                    int gx = chunk.chunkCoord.x * cs + x;
                    int gy = chunk.chunkCoord.y * cs + y;
                    int gz = chunk.chunkCoord.z * cs + z;

                    int height = groundHeight;

                    if (useNoise)
                    {
                        height = Mathf.FloorToInt(
                            Mathf.PerlinNoise(gx * noiseScale, gz * noiseScale) * 10f
                        ) + 6;
                    }

                    chunk.blocks[x, y, z] = (gy <= height) ? 1 : 0;
                }

        return 0;
    }
}
