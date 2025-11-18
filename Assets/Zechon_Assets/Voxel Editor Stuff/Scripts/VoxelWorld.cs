using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    public int worldSizeX = 4;
    public int worldSizeY = 1;
    public int worldSizeZ = 4;

    public GameObject chunkPrefab;

    VoxelChunk[,,] chunks;

    void Start()
    {
        GenerateWorld();
    }

    void GenerateWorld()
    {
        chunks = new VoxelChunk[worldSizeX, worldSizeY, worldSizeZ];

        for (int x = 0; x < worldSizeX; x++)
        {
            for (int y = 0; y < worldSizeY; y++)
            {
                for (int z = 0; z < worldSizeZ; z++)
                {
                    GameObject chunkObj = GameObject.Instantiate(chunkPrefab, transform);
                    chunkObj.name = $"Chunk {x}-{y}-{z}";
                    chunkObj.transform.parent = transform;
                    chunkObj.transform.localPosition = new Vector3(
                        x * VoxelChunk.chSize,
                        y * VoxelChunk.chSize,
                        z * VoxelChunk.chSize
                    );

                    VoxelChunk chunk = chunkObj.GetComponent<VoxelChunk>();

                    for (int ix = 0; ix < VoxelChunk.chSize; ix++)
                        for (int iy = 0; iy < VoxelChunk.chSize / 2; iy++)
                            for (int iz = 0; iz < VoxelChunk.chSize; iz++)
                                chunk.blocks[ix, iy, iz] = 1;

                    chunk.GenerateMesh();
                    chunks[x, y, z] = chunk;
                }
            }
        }
    }

    public VoxelChunk GetChunk(int x, int y, int z)
    {
        if (x < 0 || x >= worldSizeX || y < 0 || y >= worldSizeY || z < 0 || z >= worldSizeZ)
            return null;
        return chunks[x, y, z];
    }
}
