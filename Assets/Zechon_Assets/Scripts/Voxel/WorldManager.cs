using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class WorldManager : NetworkBehaviour
{
    public string worldName = "DefaultWorld";
    public VoxelWorld voxelWorld;
    public WorldGenerator generator;

    private Dictionary<Vector3Int, Chunk> loadedChunks = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        SpawnInitialChunks();
    }

    void SpawnInitialChunks()
    {
        // Example: 3x3 area
        for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
            {
                Vector3Int coord = new Vector3Int(x, 0, z);
                LoadOrGenerateChunk(coord);
            }
    }

    void LoadOrGenerateChunk(Vector3Int coord)
    {
        var loaded = SaveLoadManager.LoadChunk(worldName, coord);

        Chunk chunk = voxelWorld.CreateChunk(coord);

        if (loaded.HasValue)
        {
            chunk.SetBlocksFromServer(loaded.Value.blocks);
        }
        else
        {
            generator.FillChunk(voxelWorld, chunk);
            chunk.SetBlocksFromServer(chunk.blocks);
            SaveLoadManager.SaveChunk(worldName, chunk);
        }

        loadedChunks.Add(coord, chunk);
    }
}
