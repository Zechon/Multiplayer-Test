using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "newBlockClass", menuName = "Block Class")]
public class BlockClass : ScriptableObject
{
    public string blockName;

    [Tooltip("side, top, bottom")]
    public Texture2D[] blockFaceTextures;

    [Tooltip("side, top, bottom normal maps")]
    public Texture2D[] blockFaceNormalMaps;

    [Header("Special Rendering")]
    public bool isSlab = false;

    [Tooltip("Vertical size of the block (1 = full block, 0.5 = slab)")]
    [Range(0.5f, 1f)]
    public float height = 1f;

    [Tooltip("1 = full block(32px). 0.5 = slab(16px)")]
    public float uvSideHeight = 1f;
    public float uvTopHeight = 1f;
    public float uvBottomHeight = 1f;
}
