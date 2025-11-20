using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "newBlockClass", menuName = "Block Class")]
public class BlockClass : ScriptableObject
{
    public string blockName;

    [Tooltip("front, back, right, left, top, bottom")]
    public Texture2D[] blockFaceTextures;
}
