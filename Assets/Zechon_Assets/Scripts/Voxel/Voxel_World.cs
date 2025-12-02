using System.Collections.Generic;
using UnityEngine;

public class VoxelWorld : MonoBehaviour
{
    public static VoxelWorld Instance { get; private set; }

    [Header("World Settings")]
    public string worldName = "DefaultWorld";
    public Vector3Int initialSize = new Vector3Int(4, 1, 4);

    [Header("Voxel Systems")]
    public WorldGenerator generator;
    public BlockManager blockManager;
    public Material blockMaterial;

    [Header("Chunk Settings")]
    public int chunkSize = 16;
    public float cubeSize = 0.5f;

    // All loaded chunks
    private Dictionary<Vector3Int, Chunk> chunks = new();

    // World manifest
    private WorldManifest manifest;

    private void Awake()
    {
        Instance = this;

        // Load or create manifest
        manifest = WorldManifestManager.LoadManifest(worldName) ?? new WorldManifest { worldName = worldName };
    }

    private void Start()
    {
        Vector3Int halfSize = new Vector3Int(initialSize.x / 2, initialSize.y / 2, initialSize.z / 2);

        List<Vector3Int> allChunkCoords = new List<Vector3Int>();

        if (manifest.chunkCoordinates.Count > 0)
            allChunkCoords.AddRange(manifest.chunkCoordinates);
        else
        {
            for (int x = -halfSize.x; x < initialSize.x - halfSize.x; x++)
                for (int y = -halfSize.y; y < initialSize.y - halfSize.y; y++)
                    for (int z = -halfSize.z; z < initialSize.z - halfSize.z; z++)
                        allChunkCoords.Add(new Vector3Int(x, y, z));
        }

        foreach (var coord in allChunkCoords)
            CreateChunkObjectOnly(coord);
  
        if (manifest.chunkCoordinates.Count == 0)
        {
            foreach (var coord in allChunkCoords)
                WorldManifestManager.AddChunk(manifest, coord);

            WorldManifestManager.SaveManifest(manifest);
        }

        foreach (var chunk in chunks.Values)
        {
            chunk.GenerateChunkMesh();
            chunk.ApplyMesh();
        }
    }

    #region Chunk Management

    public Chunk GetChunk(Vector3Int coord)
    {
        chunks.TryGetValue(coord, out Chunk chunk);
        return chunk;
    }

    public Chunk CreateChunkObjectOnly(Vector3Int coord)
    {
        if (chunks.ContainsKey(coord))
            return chunks[coord];

        GameObject go = new GameObject($"Chunk_{coord.x}_{coord.y}_{coord.z}");
        go.transform.parent = transform;
        go.transform.position = new Vector3(coord.x, coord.y, coord.z) * chunkSize * cubeSize;

        Chunk chunk = go.AddComponent<Chunk>();
        chunk.chunkCoord = coord;
        chunk.cubeSize = cubeSize;
        chunk.blockManager = blockManager;

        MeshFilter mf = go.GetComponent<MeshFilter>() ?? go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.GetComponent<MeshRenderer>() ?? go.AddComponent<MeshRenderer>();
        MeshCollider mc = go.GetComponent<MeshCollider>() ?? go.AddComponent<MeshCollider>();

        Mesh mesh = new Mesh();
        chunk.mesh = mesh;
        mf.sharedMesh = mesh;

        // Assign material
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

        // Load saved chunk if exists
        var loaded = SaveLoadManager.LoadChunk(worldName, coord);
        if (loaded.HasValue)
        {
            chunk.blocks = loaded.Value.blocks;
            chunk.metadata = loaded.Value.meta;
        }
        else
        {
            generator?.FillChunk(this, chunk);

            WorldManifestManager.AddChunk(manifest, coord);
            WorldManifestManager.SaveManifest(manifest);
        }

        chunks[coord] = chunk;

        chunk.gameObject.layer = LayerMask.NameToLayer("Ground");

        return chunk;
    }

    [ContextMenu("Save All Chunks")]
    public void SaveAllChunks()
    {
        foreach (var c in chunks.Values)
        {
            SaveLoadManager.SaveChunk(worldName, c, c.metadata);
        }

        WorldManifestManager.SaveManifest(manifest);
    }

    #endregion

    #region Global Voxel Access

    public int GetBlock(int gx, int gy, int gz)
    {
        GlobalToChunkLocal(gx, gy, gz, out Vector3Int chunkCoord, out Vector3Int local);

        if (!chunks.TryGetValue(chunkCoord, out Chunk chunk))
            return 0; // missing chunk treated as air

        return chunk.blocks[local.x, local.y, local.z];
    }

    public void GlobalToChunkLocal(int gx, int gy, int gz, out Vector3Int chunkCoord, out Vector3Int local)
    {
        int cs = chunkSize;

        // Determine chunk coordinates
        chunkCoord = new Vector3Int(Mathf.FloorToInt(gx / (float)cs), Mathf.FloorToInt(gy / (float)cs), Mathf.FloorToInt(gz / (float)cs));

        // Determine local coordinates inside that chunk (always 0..chunkSize-1)
        local = new Vector3Int(((gx % cs) + cs) % cs,((gy % cs) + cs) % cs,((gz % cs) + cs) % cs);
    }

    #endregion
}
