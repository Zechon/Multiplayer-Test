using UnityEngine;
using System.Collections.Generic;

public class VoxelWorld : MonoBehaviour
{
    public static VoxelWorld Instance { get; private set; }

    public WorldGenerator generator; // plug your generator here (flat, noise, etc.)

    public BlockManager blockManager;
    public Material blockMaterial;

    // holds all loaded chunks
    private Dictionary<Vector3Int, Chunk> chunks = new();

    public int chunkSize = 16;
    public float cubeSize = 0.5f;

    void Awake()
    {
        Instance = this;
    }

    public Vector3Int initialSize = new Vector3Int(4, 1, 4); // 4×4 chunks

    void Start()
    {
        for (int x = 0; x < initialSize.x; x++)
            for (int y = 0; y < initialSize.y; y++)
                for (int z = 0; z < initialSize.z; z++)
                {
                    CreateChunk(new Vector3Int(x, y, z));
                }
    }

    public Chunk GetChunk(Vector3Int coord)
    {
        chunks.TryGetValue(coord, out Chunk c);
        return c;
    }

    public Chunk CreateChunk(Vector3Int coord)
    {
        if (chunks.ContainsKey(coord))
            return chunks[coord];

        // Create new GameObject for the chunk
        GameObject go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
        go.transform.parent = transform;

        float worldUnit = chunkSize * cubeSize;
        go.transform.position = new Vector3(
            coord.x * worldUnit,
            coord.y * worldUnit,
            coord.z * worldUnit
        );

        // Ensure required components
        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf == null) mf = go.AddComponent<MeshFilter>();

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();

        MeshCollider mc = go.GetComponent<MeshCollider>();
        if (mc == null) mc = go.AddComponent<MeshCollider>();

        // Add the chunk component
        Chunk chunk = go.AddComponent<Chunk>();
        chunk.chunkCoord = coord;
        chunk.cubeSize = cubeSize;
        chunk.blockManager = blockManager; // assign the BlockManager reference

        // Assign material if set
        if (blockMaterial != null)
        {
            mr.material = blockMaterial;
            if (blockManager != null && blockManager.atlas != null)
            {
                mr.material.mainTexture = blockManager.atlas;
                if (blockManager.normalAtlas != null)
                    mr.material.SetTexture("_BumpMap", blockManager.normalAtlas);
                chunk.numTexs = blockManager.allBlocks.Length;
            }
        }

        // Register in world dictionary
        chunks[coord] = chunk;

        // Fill voxel data using generator
        if (generator != null)
            generator.FillChunk(this, chunk);

        // Generate mesh and collider
        chunk.GenerateChunkMesh();
        chunk.ApplyMesh();

        // Assign mesh to collider
        mc.sharedMesh = chunk.GetComponent<MeshFilter>().mesh;

        return chunk;
    }


    // Global voxel lookup
    public int GetBlock(int gx, int gy, int gz)
    {
        Vector3Int c = new Vector3Int(
            Mathf.FloorToInt(gx / (float)chunkSize),
            Mathf.FloorToInt(gy / (float)chunkSize),
            Mathf.FloorToInt(gz / (float)chunkSize)
        );

        if (!chunks.TryGetValue(c, out Chunk chunk))
            return 0; // treat missing as air

        int lx = gx - c.x * chunkSize;
        int ly = gy - c.y * chunkSize;
        int lz = gz - c.z * chunkSize;

        return chunk.blocks[lx, ly, lz];
    }
}
