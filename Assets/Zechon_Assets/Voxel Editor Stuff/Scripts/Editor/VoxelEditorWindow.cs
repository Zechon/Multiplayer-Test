#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class VoxelEditorWindow : EditorWindow
{
    private bool editorEnabled = true;
    private int brushSize = 1;
    private int selectedBlock = 1;

    [MenuItem("Voxel Editor/Open Editor")]
    public static void OpenWindow()
    {
        GetWindow<VoxelEditorWindow>("Voxel Editor");
    }

    private void OnGUI()
    {
        GUILayout.Label("Voxel Editor Controls", EditorStyles.boldLabel);

        editorEnabled = EditorGUILayout.Toggle("Editor Enabled", editorEnabled);

        brushSize = EditorGUILayout.IntSlider("Brush Size", brushSize, 1, 5);

        selectedBlock = EditorGUILayout.IntField("Selected Block ID", selectedBlock);

        if (GUILayout.Button("Regenerate All Chunks"))
        {
            var chunks = GameObject.FindObjectsByType<VoxelChunk>(FindObjectsSortMode.None);
            foreach (var chunk in chunks)
            {
                chunk.GenerateMesh();
            }
        }
    }

    public bool IsEnabled() => editorEnabled;
    public int GetBrushSize() => brushSize;
    public int GetSelectedBlock() => selectedBlock;
}
#endif
