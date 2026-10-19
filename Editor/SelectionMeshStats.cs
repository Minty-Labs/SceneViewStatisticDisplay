using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[InitializeOnLoad]
public class SelectionMeshStatsWindow : EditorWindow {
    // --- Preferences Keys ---
    private const string MintyLabs_SceneMeshStats_IsVisible = "MeshStats_IsVisible";
    private const string MintyLabs_SceneMeshStats_Anchor = "MeshStats_Anchor";
    private const string MintyLabs_SceneMeshStats_ShowName = "MeshStats_ShowName";
    private const string MintyLabs_SceneMeshStats_ShowBasicMeshes = "MeshStats_ShowBasicMeshes";
    private const string MintyLabs_SceneMeshStats_ShowSkinnedMeshes = "MeshStats_ShowSkinnedMeshes";
    private const string MintyLabs_SceneMeshStats_ShowVertices = "MeshStats_ShowVertices";
    private const string MintyLabs_SceneMeshStats_ShowTriangles = "MeshStats_ShowTriangles";
    private const string MintyLabs_SceneMeshStats_ShowSubMeshes = "MeshStats_ShowSubMeshes";
    private const string MintyLabs_SceneMeshStats_UseColourCoding = "MeshStats_UseColourCoding";

    // --- State Variables ---
    private static bool _isVisible = true;
    private static int _anchorPosition = 0; // 0=TopLeft, 1=TopRight, 2=BottomLeft, 3=BottomRight
    private static bool _showName = true;
    private static bool _showBasicMeshes = true;
    private static bool _showSkinnedMeshes = true;
    private static bool _showVertices = true;
    private static bool _showTriangles = true;
    private static bool _showSubMeshes = true;
    private static bool _useColourCoding = true;

    private static bool _prefsLoaded = false;

    private readonly string[] _anchorOptions = { "Top Left", "Top Right", "Bottom Left", "Bottom Right" };

    static SelectionMeshStatsWindow() {
        // Safe to hook events here, but API calls like EditorPrefs are deferred
        SceneView.duringSceneGui -= OnSceneGUI;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnEnable() => LoadPrefs();

    private static void LoadPrefs() {
        _isVisible = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_IsVisible, true);
        _anchorPosition = EditorPrefs.GetInt(MintyLabs_SceneMeshStats_Anchor, 0);
        _showName = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_ShowName, true);
        _showBasicMeshes = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_ShowBasicMeshes, true);
        _showSkinnedMeshes = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_ShowSkinnedMeshes, true);
        _showVertices = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_ShowVertices, true);
        _showTriangles = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_ShowTriangles, true);
        _showSubMeshes = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_ShowSubMeshes, true);
        _useColourCoding = EditorPrefs.GetBool(MintyLabs_SceneMeshStats_UseColourCoding, true);

        _prefsLoaded = true;
    }

    [MenuItem("Tools/Minty Labs/Mesh Stats Settings")]
    public static void ShowWindow() => GetWindow<SelectionMeshStatsWindow>("Mesh Stats Settings").titleContent.image = EditorGUIUtility.IconContent("TilemapRenderer Icon").image;

    [Shortcut("Scene View/Toggle Mesh Stats", KeyCode.F3)]
    public static void ToggleStatsVisibility() {
        if (!_prefsLoaded) LoadPrefs();

        _isVisible = !_isVisible;
        EditorPrefs.SetBool(MintyLabs_SceneMeshStats_IsVisible, _isVisible);
        SceneView.RepaintAll();
    }

    // --- Settings GUI ---
    private void OnGUI() {
        if (!_prefsLoaded) LoadPrefs();

        GUILayout.Label("Display Settings", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();

        _isVisible = EditorGUILayout.ToggleLeft("Show Overlay (F3)", _isVisible);
        _anchorPosition = EditorGUILayout.Popup("Screen Position", _anchorPosition, _anchorOptions);
        _useColourCoding = EditorGUILayout.ToggleLeft("VRChat Performance Colouring", _useColourCoding);

        EditorGUILayout.Space();
        GUILayout.Label("Visible Statistics", EditorStyles.boldLabel);

        _showName = EditorGUILayout.ToggleLeft("Selected Object Name", _showName);
        _showBasicMeshes = EditorGUILayout.ToggleLeft("Basic Meshes (MeshRenderer)", _showBasicMeshes);
        _showSkinnedMeshes = EditorGUILayout.ToggleLeft("Skinned Meshes", _showSkinnedMeshes);
        _showVertices = EditorGUILayout.ToggleLeft("Vertex Count", _showVertices);
        _showTriangles = EditorGUILayout.ToggleLeft("Triangle Count", _showTriangles);
        _showSubMeshes = EditorGUILayout.ToggleLeft("Material Count", _showSubMeshes);

        if (EditorGUI.EndChangeCheck()) {
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_IsVisible, _isVisible);
            EditorPrefs.SetInt(MintyLabs_SceneMeshStats_Anchor, _anchorPosition);
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_ShowName, _showName);
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_ShowBasicMeshes, _showBasicMeshes);
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_ShowSkinnedMeshes, _showSkinnedMeshes);
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_ShowVertices, _showVertices);
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_ShowTriangles, _showTriangles);
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_ShowSubMeshes, _showSubMeshes);
            EditorPrefs.SetBool(MintyLabs_SceneMeshStats_UseColourCoding, _useColourCoding);

            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Press F3 to toggle the overlay in the Scene View. Customize keybind under Edit > Shortcuts.", MessageType.Info);
        
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Mesh Stats • Minty Labs • v1.1.0", EditorStyles.centeredGreyMiniLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // --- Scene View Overlay ---
    private static void OnSceneGUI(SceneView sceneView) {
        if (!_prefsLoaded) LoadPrefs();

        if (!_isVisible || Selection.activeGameObject == null)
            return;

        int vertexCount = 0, triangleCount = 0, basicMeshCount = 0, skinnedMeshCount = 0, materialCount = 0;

        var processedBasicRenderers = new HashSet<MeshRenderer>();
        var processedSkinnedRenderers = new HashSet<SkinnedMeshRenderer>();

        foreach (var obj in Selection.gameObjects) {
            // 1. Process Basic Meshes (MeshFilter + MeshRenderer)
            var meshFilters = obj.GetComponentsInChildren<MeshFilter>(true);
            foreach (var mf in meshFilters) {
                var mr = mf.GetComponent<MeshRenderer>();
                if (mf.sharedMesh == null || mr == null || processedBasicRenderers.Contains(mr)) continue;
                vertexCount += mf.sharedMesh.vertexCount;
                triangleCount += mf.sharedMesh.triangles.Length / 3;
                materialCount += mf.sharedMesh.subMeshCount;
                basicMeshCount++;
                processedBasicRenderers.Add(mr);
            }

            // 2. Process Skinned Meshes
            var skinnedMeshes = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (var smr in skinnedMeshes) {
                if (smr.sharedMesh == null || processedSkinnedRenderers.Contains(smr)) continue;
                vertexCount += smr.sharedMesh.vertexCount;
                triangleCount += smr.sharedMesh.triangles.Length / 3;
                materialCount += smr.sharedMesh.subMeshCount;
                skinnedMeshCount++;
                processedSkinnedRenderers.Add(smr);
            }
        }

        // Calculate dynamic height based on toggled stats
        const float boxWidth = 230f;
        var boxHeight = 25f;
        if (_showName) boxHeight += 20f;
        if (_showBasicMeshes) boxHeight += 20f;
        if (_showSkinnedMeshes) boxHeight += 20f;
        if (_showVertices) boxHeight += 20f;
        if (_showTriangles) boxHeight += 20f;
        if (_showSubMeshes) boxHeight += 20f;

        const float padding = 10f;
        var xPos = padding;
        var yPos = padding;

        switch (_anchorPosition) {
            case 0:
                xPos = padding;
                yPos = padding;
                break; // Top Left
            case 1:
                xPos = sceneView.position.width - boxWidth - padding;
                yPos = padding;
                break; // Top Right
            case 2:
                xPos = padding;
                yPos = sceneView.position.height - boxHeight - 35f;
                break; // Bottom Left
            case 3:
                xPos = sceneView.position.width - boxWidth - padding;
                yPos = sceneView.position.height - boxHeight - 35f;
                break; // Bottom Right
        }

        Handles.BeginGUI();

        var uiRect = new Rect(xPos, yPos, boxWidth, boxHeight);
        GUILayout.BeginArea(uiRect, "Mesh Stats", GUI.skin.window);

        var labelStyle = new GUIStyle(EditorStyles.label) {
            fontSize = 12,
            fontStyle = FontStyle.Normal,
            richText = true
        };

        var boldStyle = new GUIStyle(EditorStyles.label) {
            fontSize = 12,
            fontStyle = FontStyle.Bold,
            richText = true
        };

        if (_showName) {
            var nameLabel = Selection.gameObjects.Length > 1
                ? $"Multiple Objects ({Selection.gameObjects.Length})"
                : Selection.activeGameObject.name;
            GUILayout.Label(nameLabel, boldStyle);
        }

        if (_showBasicMeshes) {
            var valStr = basicMeshCount.ToString();
            if (_useColourCoding)
                valStr = $"<color={GetBasicMeshColour(basicMeshCount)}>{valStr}</color>";
            GUILayout.Label($"Basic Meshes: {valStr}", labelStyle);
        }

        if (_showSkinnedMeshes) {
            var valStr = skinnedMeshCount.ToString();
            if (_useColourCoding)
                valStr = $"<color={GetSkinnedMeshColour(skinnedMeshCount)}>{valStr}</color>";
            GUILayout.Label($"Skinned Meshes: {valStr}", labelStyle);
        }

        if (_showVertices) {
            GUILayout.Label($"Vertices: {vertexCount:N0}", labelStyle);
        }

        if (_showTriangles) {
            var valStr = $"{triangleCount:N0}";
            if (_useColourCoding)
                valStr = $"<color={GetTriangleColour(triangleCount)}>{valStr}</color>";
            GUILayout.Label($"Triangles: {valStr}", labelStyle);
        }

        if (_showSubMeshes) {
            var valStr = $"{materialCount:N0}";
            if (_useColourCoding)
                valStr = $"<color={GetMaterialColour(materialCount)}>{valStr}</color>";
            GUILayout.Label($"Materials: {valStr}", labelStyle);
        }

        GUILayout.EndArea();
        Handles.EndGUI();
    }

    // --- VRChat Rating Hex Colour Evaluators ---
    private static string GetTriangleColour(int count) {
        return count switch {
            <= 32000 => "#46a137",
            <= 70000 => "#6ae854",
            _ => "#d13d3d"
        };
    }

    private static string GetSkinnedMeshColour(int count) {
        return count switch {
            <= 1 => "#46a137",
            <= 2 => "#6ae854",
            <= 8 => "#EAAA09",
            <= 16 => "#E55A42",
            _ => "#d13d3d"
        };
    }

    private static string GetBasicMeshColour(int count) {
        return count switch {
            <= 4 => "#46a137",
            <= 8 => "#6ae854",
            <= 16 => "#EAAA09",
            <= 32 => "#E55A42",
            _ => "#d13d3d"
        };
    }
    
    private static string GetMaterialColour(int count) {
        return count switch {
            <= 4 => "#46a137",
            <= 8 => "#6ae854",
            <= 16 => "#EAAA09",
            <= 32 => "#E55A42",
            _ => "#d13d3d"
        };
    }
}