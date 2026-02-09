using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class VoxelWorld : NetworkBehaviour
{
    public static VoxelWorld Instance { get; private set; }

    [Header("World Settings")]
    public string worldName = "DefaultWorld";
    public Vector3Int initialSize = new Vector3Int(4, 1, 4); // x,y,z chunks

    [Header("Voxel Systems")]
    public WorldGenerator generator;
    public BlockManager blockManager;
    public Material blockMaterial;

    [Header("Networking")]
    public Chunk chunkPrefab;

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
        if (!IsServer) return;

        Vector3Int halfSize = new Vector3Int(initialSize.x / 2, initialSize.y / 2, initialSize.z / 2);

        // Load all chunks in manifest
        foreach (var coord in manifest.chunkCoordinates)
        {
            CreateChunk(coord);
        }

        // If manifest empty, generate initial chunks
        if (manifest.chunkCoordinates.Count == 0)
        {
            for (int x = -halfSize.x; x < initialSize.x - halfSize.x; x++)
                for (int y = -halfSize.y; y < initialSize.y - halfSize.y; y++)
                    for (int z = -halfSize.z; z < initialSize.z - halfSize.z; z++)
                    {
                        Vector3Int coord = new Vector3Int(x, y, z);
                        CreateChunk(coord);
                    }

            WorldManifestManager.SaveManifest(manifest);
        }
    }

    #region Chunk Management

    public Chunk GetChunk(Vector3Int coord)
    {
        chunks.TryGetValue(coord, out Chunk chunk);
        return chunk;
    }

    public Chunk CreateChunk(Vector3Int coord)
    {
        if (!IsServer)
            return null;

        if (chunks.TryGetValue(coord, out var existing))
            return existing;

        Chunk chunk = Instantiate(chunkPrefab, transform);

        chunk.chunkCoord = coord;
        chunk.cubeSize = cubeSize;
        chunk.blockManager = blockManager;
        chunk.numTexs = blockManager.allBlocks.Length;

        chunk.transform.position =
            new Vector3(coord.x, coord.y, coord.z) * chunkSize * cubeSize;

        NetworkObject netObj = chunk.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        chunks.Add(coord, chunk);
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
        Vector3Int c = new Vector3Int(
            Mathf.FloorToInt(gx / (float)chunkSize),
            Mathf.FloorToInt(gy / (float)chunkSize),
            Mathf.FloorToInt(gz / (float)chunkSize)
        );

        if (!chunks.TryGetValue(c, out Chunk chunk))
            return 0;

        int lx = gx - c.x * chunkSize;
        int ly = gy - c.y * chunkSize;
        int lz = gz - c.z * chunkSize;

        return chunk.blocks[lx, ly, lz];
    }

    #endregion
}
