using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "newBlockClass", menuName = "Block Class")]
public class BlockClass : ScriptableObject
{
    public string blockName;

    [Tooltip("side, top, bottom")]
    public Texture2D[] blockFaceTextures;

    [Tooltip("side, top, bottom normal maps")]
    public Texture2D[] blockFaceNormalMaps;
}
