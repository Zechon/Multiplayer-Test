using UnityEngine;

public class VoxelWorldRef : MonoBehaviour
{
    public static VoxelWorldRef Instance;

    public VoxelWorld world;

    void Awake()
    {
        Instance = this;
    }

    public VoxelChunk GetChunkAtWorldPos(Vector3Int pos)
    {
        int cx = Mathf.FloorToInt(pos.x / (float)VoxelChunk.chSize);
        int cy = Mathf.FloorToInt(pos.y / (float)VoxelChunk.chSize);
        int cz = Mathf.FloorToInt(pos.z / (float)VoxelChunk.chSize);

        return world.GetChunk(cx, cy, cz);
    }

    public Vector3Int WorldToLocalVoxel(Vector3Int pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(Mathf.Repeat(pos.x, VoxelChunk.chSize)),
            Mathf.FloorToInt(Mathf.Repeat(pos.y, VoxelChunk.chSize)),
            Mathf.FloorToInt(Mathf.Repeat(pos.z, VoxelChunk.chSize))
        );
    }
}
