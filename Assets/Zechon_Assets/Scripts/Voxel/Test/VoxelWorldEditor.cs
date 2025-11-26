using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;

public class VoxelWorldEditor : MonoBehaviour
{
    [Header("Editor Settings")]
    public Camera editorCamera;
    public LayerMask chunkLayerMask;
    public int brushSize = 1;
    public int selectedBlockID = 1;
    private int BlockIDMax;
    [SerializeField] float placeDelay = 0.075f;
    float lastPlaceTime = 0f;

    [Header("Highlight Preview")]
    public GameObject previewPrefab;
    private GameObject previewInstance;
    private Vector3Int? previewBlockLocal; 
    private Chunk previewChunk;

    [Header("UI")]
    public TMP_Text IdText;

    [Header("Other References")]
    [SerializeField] private BlockManager blockManager;

    // Queue for async chunk loading
    private Queue<(Vector3Int coord, (ChunkMetadata meta, int[,,] blocks) data)> applyQueue = new();

    // Undo/Redo
    private struct BlockEdit
    {
        public List<(Vector3Int worldPos, int previousID, int newID)> changes;
    }
    private Stack<BlockEdit> undoStack = new();
    private Stack<BlockEdit> redoStack = new();

    private void Start()
    {
        previewInstance = Instantiate(previewPrefab);
        previewInstance.SetActive(false);

        BlockIDMax = blockManager.allBlocks.Length - 1;

        string result = $"ID: {selectedBlockID}, {blockManager.allBlocks[selectedBlockID]}";
        IdText.text = result.Replace("(BlockClass)", " ");
    }

    private void Update()
    {
        HandleInput();
        ApplyQueuedChunks();
        UpdatePreview();
    }

    #region Async Chunk Queue

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

        if (Input.GetMouseButton(0) && CanPlace()) ModifyBlock(true);
        if (Input.GetMouseButton(1) && CanPlace()) ModifyBlock(false);

        if (Input.GetKeyDown(KeyCode.Z)) Undo();
        if (Input.GetKeyDown(KeyCode.Y)) Redo();

        if (Input.GetKeyDown(KeyCode.E)) SwapSelectedBlock(1);
        if (Input.GetKeyDown(KeyCode.Q)) SwapSelectedBlock(-1);
    }

    private void SwapSelectedBlock(int change)
    {
        if (selectedBlockID == BlockIDMax && change == 1) selectedBlockID = 1;
        else if (selectedBlockID == 1 && change == -1) selectedBlockID = BlockIDMax;
        else selectedBlockID += change;

        string result = $"ID: {selectedBlockID}, {blockManager.allBlocks[selectedBlockID]}";
        IdText.text =  result.Replace("(BlockClass)", " ");
    }

    private void ModifyBlock(bool place)
    {
        Ray ray = editorCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, 15f, chunkLayerMask)) return;

        Vector3 targetPos;

        if (place)
        {
            // Place block next to the face you hit
            targetPos = hit.point + hit.normal * (VoxelWorld.Instance.cubeSize * 0.5f);
        }
        else
        {
            // Remove block being looked at
            targetPos = hit.point - hit.normal * 0.01f;
        }

        Vector3Int worldBlock = Vector3Int.FloorToInt(targetPos / VoxelWorld.Instance.cubeSize);

        ApplyBrushWorld(worldBlock, place);
    }

    private void ApplyBrushWorld(Vector3Int anchor, bool place)
    {
        int cs = VoxelWorld.Instance.chunkSize;

        var edit = new BlockEdit { changes = new List<(Vector3Int worldPos, int previousID, int newID)>() };

        for (int x = 0; x < brushSize; x++)
            for (int y = 0; y < brushSize; y++)
                for (int z = 0; z < brushSize; z++)
                {
                    Vector3Int pos = anchor + new Vector3Int(x, y, z);

                    // Determine chunk
                    Vector3Int chunkCoord = new Vector3Int(
                        Mathf.FloorToInt(pos.x / (float)cs),
                        Mathf.FloorToInt(pos.y / (float)cs),
                        Mathf.FloorToInt(pos.z / (float)cs)
                    );

                    Chunk chunk = VoxelWorld.Instance.GetChunk(chunkCoord);
                    if (chunk == null) chunk = VoxelWorld.Instance.CreateChunk(chunkCoord);

                    Vector3Int localPos = pos - chunkCoord * cs;
                    if (!IsInsideChunk(localPos)) continue;

                    int previousID = chunk.blocks[localPos.x, localPos.y, localPos.z];
                    int newID = place ? selectedBlockID : 0;

                    if (previousID == newID) continue; // skip no-op

                    chunk.blocks[localPos.x, localPos.y, localPos.z] = newID;
                    edit.changes.Add((pos, previousID, newID));

                    chunk.GenerateChunkMesh();
                    chunk.ApplyMesh();

                    _ = SaveLoadManager.SaveChunkAsync(VoxelWorld.Instance.worldName, chunk, chunk.metadata);
                }

        if (edit.changes.Count > 0)
        {
            undoStack.Push(edit);
            redoStack.Clear();
        }
    }


    private bool IsInsideChunk(Vector3Int pos) { int cs = VoxelWorld.Instance.chunkSize; return pos.x >= 0 && pos.y >= 0 && pos.z >= 0 && pos.x < cs && pos.y < cs && pos.z < cs; }

    private void Undo()
    {
        if (undoStack.Count == 0) return;
        var edit = undoStack.Pop();
        redoStack.Push(edit);
        ApplyEdit(edit, undo: true);
    }

    private void Redo()
    {
        if (redoStack.Count == 0) return;
        var edit = redoStack.Pop();
        undoStack.Push(edit);
        ApplyEdit(edit, undo: false);
    }

    private void ApplyEdit(BlockEdit edit, bool undo)
    {
        foreach (var change in edit.changes)
        {
            Vector3Int pos = change.worldPos;
            Vector3Int chunkCoord = new Vector3Int(
                Mathf.FloorToInt(pos.x / (float)VoxelWorld.Instance.chunkSize),
                Mathf.FloorToInt(pos.y / (float)VoxelWorld.Instance.chunkSize),
                Mathf.FloorToInt(pos.z / (float)VoxelWorld.Instance.chunkSize)
            );
            Chunk chunk = VoxelWorld.Instance.GetChunk(chunkCoord);
            if (chunk == null) continue;

            Vector3Int localPos = pos - chunkCoord * VoxelWorld.Instance.chunkSize;
            chunk.blocks[localPos.x, localPos.y, localPos.z] = undo ? change.previousID : change.newID;
            chunk.GenerateChunkMesh();
            chunk.ApplyMesh();
            _ = SaveLoadManager.SaveChunkAsync(VoxelWorld.Instance.worldName, chunk, chunk.metadata);
        }
    }

    #endregion

    #region Highlight Preview

    private void UpdatePreview()
    {
        Ray ray = editorCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (!Physics.Raycast(ray, out RaycastHit hit, 100f, chunkLayerMask))
        {
            previewInstance.SetActive(false);
            previewBlockLocal = null;
            return;
        }

        Vector3 hitPosInside = hit.point - hit.normal * 0.01f;
        Vector3Int anchorBlock = Vector3Int.FloorToInt(hitPosInside / VoxelWorld.Instance.cubeSize);

        previewInstance.transform.position = (Vector3)anchorBlock * VoxelWorld.Instance.cubeSize
                                             + Vector3.one * (VoxelWorld.Instance.cubeSize / 2f);

        float scale = (VoxelWorld.Instance.cubeSize * brushSize) + 0.01f;
        previewInstance.transform.localScale = new Vector3(scale, scale, scale);

        previewInstance.SetActive(true);
        previewBlockLocal = anchorBlock;
    }

    #endregion

    private bool CanPlace()
    {
        if (Time.time - lastPlaceTime < placeDelay) return false;
        lastPlaceTime = Time.time;
        return true;
    }
}
