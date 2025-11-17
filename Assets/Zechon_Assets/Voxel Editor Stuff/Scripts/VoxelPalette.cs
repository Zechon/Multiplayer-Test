using UnityEngine;

[CreateAssetMenu(menuName = "Voxels/ Voxel Palette", fileName = "VoxelPalette")]
public class VoxelPalette : ScriptableObject
{
    public VoxelType[] types;

    [System.Serializable]
    public class VoxelType
    {
        public string name;
        public Color color = Color.white;
        public Material material;
        public bool isSolid = true;
        public bool destructible = true;
    }

    public VoxelType Get(int id)
    {
        if (id < 0 || id >= types.Length) return null;
        return types[id];
    }
}
