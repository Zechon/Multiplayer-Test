using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class VoxelWorldEditor : MonoBehaviour
{
    [Header("Editor Settings")]
    public Camera editorCamera;
    public LayerMask chunkLayerMask;
    public int brushSize = 1;
    public int selectedBlockID = 1;

    // Queue for applying loaded chunks on main thread
    private Queue<(Vector3Int coord, (ChunkMetadata meta, int[,,] blocks) data)> applyQueue = new();

    private void Update()
    {
        HandleInput();
        ApplyQueuedChunks();
    }

    #region Async Loading Queue

    private void ApplyQueuedChunks()
    {
        while (applyQueue.Count > 0)
        {
            var item = applyQueue.Dequeue();
            Chunk chunk = VoxelWorld.Instance.CreateChunk(item.coord);
            chunk.blocks = item.data.blocks;
            chunk.metadata = item.data.meta;
            chunk.GenerateChunkMesh();
            chunk.ApplyMesh();
        }
    }

    public async Task LoadChunkEditor(Vector3Int coord)
    {
        var data = await SaveLoadManager.LoadChunkAsync(VoxelWorld.Instance.worldName, coord);
        if (data.HasValue)
        {
            applyQueue.Enqueue((coord, data.Value));
        }
    }

    #endregion

    #region Block Editing

    private void HandleInput()
    {
        if (editorCamera == null) return;

        if (Input.GetMouseButton(0)) // Left-click: place
        {
            ModifyBlock(true);
        }
        else if (Input.GetMouseButton(1)) // Right-click: remove
        {
            ModifyBlock(false);
        }
    }

    private void ModifyBlock(bool place)
    {
        Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, chunkLayerMask)) return;

        Vector3Int chunkCoord = new Vector3Int(
            Mathf.FloorToInt(hit.point.x / VoxelWorld.Instance.chunkSize),
            Mathf.FloorToInt(hit.point.y / VoxelWorld.Instance.chunkSize),
            Mathf.FloorToInt(hit.point.z / VoxelWorld.Instance.chunkSize)
        );

        Chunk chunk = VoxelWorld.Instance.GetChunk(chunkCoord);
        if (chunk == null) return;

        Vector3 localPos = hit.point - chunk.transform.position;
        Vector3Int blockLocal = new Vector3Int(
            Mathf.FloorToInt(localPos.x / chunk.cubeSize),
            Mathf.FloorToInt(localPos.y / chunk.cubeSize),
            Mathf.FloorToInt(localPos.z / chunk.cubeSize)
        );

        ApplyBrush(chunk, blockLocal, place);
    }

    private void ApplyBrush(Chunk chunk, Vector3Int center, bool place)
    {
        int radius = Mathf.Max(brushSize, 1);

        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                for (int z = -radius; z <= radius; z++)
                {
                    Vector3Int pos = center + new Vector3Int(x, y, z);
                    if (IsInsideChunk(pos))
                    {
                        chunk.blocks[pos.x, pos.y, pos.z] = place ? selectedBlockID : 0;
                    }
                }

        chunk.GenerateChunkMesh();
        chunk.ApplyMesh();

        // Save asynchronously
        _ = SaveLoadManager.SaveChunkAsync(VoxelWorld.Instance.worldName, chunk, chunk.metadata);
    }

    private bool IsInsideChunk(Vector3Int pos)
    {
        int cs = VoxelWorld.Instance.chunkSize;
        return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 &&
               pos.x < cs && pos.y < cs && pos.z < cs;
    }

    #endregion
}
