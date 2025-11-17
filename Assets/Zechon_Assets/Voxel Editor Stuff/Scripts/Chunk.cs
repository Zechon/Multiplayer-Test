using Unity.Networking.Transport;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public const int SIZE = 16;
    public int posX, posY, posZ;
    public VoxelPalette palette;

    [HideInInspector]
    public byte[,,] blocks = new byte[SIZE, SIZE, SIZE];

    private MeshFilter mf;
    private MeshRenderer mr;
    private MeshCollider mc;
    private MeshBuilder mb = new MeshBuilder();

    private void Awake()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        mc = GetComponent<MeshCollider>();

        if (mr != null && palette != null && palette.types.Length > 0 && palette.types[1] != null)
        {

        }
    }

    public void SetBlock(int x, int y, int z, byte id, bool updateMesh = true)
    {
        if (!InBounds(x, y, z)) return;
        blocks[x, y, z] = id;
        if (updateMesh) BuildMesh();
    }

    public byte GetBlock(int x, int y, int z)
    {
        if (!InBounds(x, y, z)) return 0;
        return blocks[x, y, z];
    }

    private bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < SIZE && y >= 0 && y < SIZE && z >= 0 && z < SIZE;
    }

    // call to (re)build the chunk mesh
    public void BuildMesh()
    {
        mb.Clear();

        for (int x = 0; x < SIZE; x++)
            for (int y = 0; y < SIZE; y++)
                for (int z = 0; z < SIZE; z++)
                {
                    byte id = blocks[x, y, z];
                    if (id == 0) continue;
                    VoxelPalette.VoxelType vt = palette.Get(id);
                    Color col = vt != null ? vt.color : Color.magenta;

                    Vector3 basePos = new Vector3(x, y, z);

                    // add faces that face air or out-of-bounds
                    // left
                    if (!HasSolidAt(x - 1, y, z)) mb.AddQuad(basePos + new Vector3(0, 0, 0), basePos + new Vector3(0, 0, 1), basePos + new Vector3(0, 1, 1), basePos + new Vector3(0, 1, 0), col);
                    // right
                    if (!HasSolidAt(x + 1, y, z)) mb.AddQuad(basePos + new Vector3(1, 0, 1), basePos + new Vector3(1, 0, 0), basePos + new Vector3(1, 1, 0), basePos + new Vector3(1, 1, 1), col);
                    // down
                    if (!HasSolidAt(x, y - 1, z)) mb.AddQuad(basePos + new Vector3(0, 0, 1), basePos + new Vector3(1, 0, 1), basePos + new Vector3(1, 0, 0), basePos + new Vector3(0, 0, 0), col);
                    // up
                    if (!HasSolidAt(x, y + 1, z)) mb.AddQuad(basePos + new Vector3(0, 1, 0), basePos + new Vector3(1, 1, 0), basePos + new Vector3(1, 1, 1), basePos + new Vector3(0, 1, 1), col);
                    // back
                    if (!HasSolidAt(x, y, z - 1)) mb.AddQuad(basePos + new Vector3(1, 0, 0), basePos + new Vector3(0, 0, 0), basePos + new Vector3(0, 1, 0), basePos + new Vector3(1, 1, 0), col);
                    // forward
                    if (!HasSolidAt(x, y, z + 1)) mb.AddQuad(basePos + new Vector3(0, 0, 1), basePos + new Vector3(1, 0, 1), basePos + new Vector3(1, 1, 1), basePos + new Vector3(0, 1, 1), col);
                }

        Mesh mesh = mb.ToMesh();
        mf.sharedMesh = mesh;
        if (mc != null)
        {
            mc.sharedMesh = mesh;
        }
    }

    private bool HasSolidAt(int x, int y, int z)
    {
        if (!InBounds(x, y, z)) return false;
        byte id = blocks[x, y, z];
        if (id == 0) return false;
        VoxelPalette.VoxelType vt = palette.Get(id);
        return vt != null && vt.isSolid;
    }
}