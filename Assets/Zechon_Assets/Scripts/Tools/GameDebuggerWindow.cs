using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameDebuggerWindow : EditorWindow
{
    private Vector2 scroll;
    private Dictionary<string, bool> foldoutStates
        = new Dictionary<string, bool>();

    [MenuItem("Tools/Game Debugger")]
    public static void ShowWindow()
    {
        GetWindow<GameDebuggerWindow>("System Debugger");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Force Refresh"))
        {
            Repaint();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        List<DebugSection> sections = GameDebugRegistry.GetSections();

        foreach (var section in sections)
        {
            DrawSection(section, 0);
        }

        EditorGUILayout.EndScrollView();
    }

    private void Update()
    {
        // Live updating while playing
        if (Application.isPlaying)
            Repaint();
    }

    private void DrawSection(DebugSection section, int indent)
    {
        EditorGUI.indentLevel = indent;

        if (!foldoutStates.ContainsKey(section.Title))
            foldoutStates[section.Title] = true;

        foldoutStates[section.Title] = EditorGUILayout.Foldout(
            foldoutStates[section.Title],
            section.Title,
            true);

        if (foldoutStates[section.Title])
        {
            if (section.ContentProvider != null)
            {
                EditorGUILayout.LabelField(
                    section.ContentProvider.Invoke(),
                    EditorStyles.helpBox);
            }

            foreach (var child in section.Children)
            {
                DrawSection(child, indent + 1);
            }
        }
    }
}
