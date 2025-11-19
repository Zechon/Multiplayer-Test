using UnityEngine;

[CreateAssetMenu(fileName = "NewBlockData", menuName = "Voxel/Block Data")]
public class BlockData : ScriptableObject
{
    public string blockName;

    [Header("Textures")]
    public Texture2D topTexture;
    public Texture2D bottomTexture;
    public Texture2D sideTexture;

    [Header("Normal Maps (optional)")]
    public Texture2D topNormal;
    public Texture2D bottomNormal;
    public Texture2D sideNormal;

    public bool isSolid = true;

    [HideInInspector] public int topTexIndex = -1;
    [HideInInspector] public int bottomTexIndex = -1;
    [HideInInspector] public int sideTexIndex = -1;

    [HideInInspector] public int topNormalIndex = -1;
    [HideInInspector] public int bottomNormalIndex = -1;
    [HideInInspector] public int sideNormalIndex = -1;

    public int GetTextureIndex(Face face)
    {
        switch (face)
        {
            case Face.Top: return topTexIndex;
            case Face.Bottom: return bottomTexIndex;
            default: return sideTexIndex;
        }
    }

    public int GetNormalIndex(Face face)
    {
        switch (face)
        {
            case Face.Top: return topNormalIndex;
            case Face.Bottom: return bottomNormalIndex;
            default: return sideNormalIndex;
        }
    }
}

public enum Face { Top, Bottom, Side }
