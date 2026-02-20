using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameDebuggerWindow : EditorWindow
{
    private Vector2 scroll;
    private List<DebugSection> snapshot = new List<DebugSection>();
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    private double nextRefreshTime;
    private const double refreshRate = 0.25; // 4 times per second

    [MenuItem("Tools/Game Debugger")]
    public static void ShowWindow()
    {
        GetWindow<GameDebuggerWindow>("Game System Debugger");
    }

    private void OnEnable()
    {
        nextRefreshTime = EditorApplication.timeSinceStartup;
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        if (EditorApplication.timeSinceStartup < nextRefreshTime)
            return;

        nextRefreshTime = EditorApplication.timeSinceStartup + refreshRate;

        snapshot = GameDebugRegistry.BuildSnapshot();
        Repaint();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh"))
        {
            snapshot = GameDebugRegistry.BuildSnapshot();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var section in snapshot)
        {
            DrawSection(section, 0);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawSection(DebugSection section, int indent)
    {
        EditorGUI.indentLevel = indent;

        if (!foldouts.ContainsKey(section.Id))
            foldouts[section.Id] = true;

        foldouts[section.Id] =
            EditorGUILayout.Foldout(foldouts[section.Id], section.Title, true);

        if (foldouts[section.Id])
        {
            if (!string.IsNullOrEmpty(section.Content))
            {
                EditorGUILayout.LabelField(section.Content, EditorStyles.helpBox);
            }

            section.Children.Sort((a, b) => a.Order.CompareTo(b.Order));

            foreach (var child in section.Children)
            {
                DrawSection(child, indent + 1);
            }
        }
    }
}