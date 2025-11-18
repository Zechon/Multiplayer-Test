#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class VoxelEditorTool
{
    static VoxelEditorWindow window;

    static VoxelEditorTool()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        window = EditorWindow.GetWindow<VoxelEditorWindow>();
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        if (window == null || !window.IsEnabled()) return;

        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Handles.color = Color.yellow;
            Vector3 p = hit.point - hit.normal * 0.01f;
            Vector3Int voxelPos = Vector3Int.FloorToInt(p);

            Handles.DrawWireCube(voxelPos + Vector3.one * 0.5f, Vector3.one);

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlaceVoxel(voxelPos);
                e.Use();
            }
            if (e.type == EventType.MouseDown && e.button == 1)
            {
                RemoveVoxel(voxelPos);
                e.Use();
            }
        }

    static void PlaceVoxel(Vector3Int pos)
        {
            VoxelChunk ch = VoxelWorldRef.Instance.GetChunkAtWorldPos(pos);

            if (ch == null) return;

            Vector3Int local = VoxelWorldRef.Instance.WorldToLocalVoxel(pos);
            Undo.RegisterCompleteObjectUndo(ch, "Add Voxel");

            ch.blocks[local.x, local.y, local.z] = 1;
            ch.GenerateMesh();
        }

        static void RemoveVoxel(Vector3Int pos)
        {
            VoxelChunk ch = VoxelWorldRef.Instance.GetChunkAtWorldPos(pos);

            if (ch == null) return;

            Vector3Int local = VoxelWorldRef.Instance.WorldToLocalVoxel(pos);
            Undo.RegisterCompleteObjectUndo(ch, "Remove Voxel");

            ch.blocks[local.x, local.y, local.z] = 0;
            ch.GenerateMesh();
        }
    }
}
#endif