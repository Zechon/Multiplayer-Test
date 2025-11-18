using UnityEngine;

[CreateAssetMenu(fileName = "BlockDatabase", menuName = "Voxel/Block Database")]
public class BlockDatabase : ScriptableObject
{
    public BlockData[] blocks;

    public BlockData GetBlock(int id)
    {
        if (id < 0 || id >= blocks.Length) return null;
        return blocks[id];
    }
}
