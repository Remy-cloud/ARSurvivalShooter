#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Editor-only helper that builds project assets for the AR Survival Shooter.
/// Menu: Tools > AR Shooter > ...
/// </summary>
public static class ARShooterSetup
{
    const string Root        = "Assets";
    const string ScenePath   = "Assets/Scenes/ARGame.unity";
    const string TexturePath = Root + "/Textures/PlaneTexture_IntwaliRemy.png";
    const string MatPath     = Root + "/Materials/M_CustomPlane.mat";
    const string PrefabPath  = Root + "/Prefabs/AR/CustomARPlane.prefab";

    [MenuItem("Tools/AR Shooter/Step 2.2 - Scene + Custom Plane")]
    public static void SetupCustomPlane()
    {
        EnsureFolders("Scenes", "Materials", "Prefabs/AR", "Textures");

        // 1. Scene: copy the template SampleScene into our own ARGame scene (only once).
        if (!System.IO.File.Exists(ScenePath))
            AssetDatabase.CopyAsset("Assets/Scenes/SampleScene.unity", ScenePath);

        // 2. Texture import settings (transparent, tiling).
        var ti = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        ti.textureType = TextureImporterType.Default;
        ti.alphaIsTransparency = true;
        ti.wrapMode = TextureWrapMode.Repeat;
        ti.mipmapEnabled = true;
        ti.anisoLevel = 4;
        ti.SaveAndReimport();
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);

        // 3. Transparent URP Unlit material.
        var mat = AssetDatabase.LoadAssetAtPath<Material>(MatPath);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            AssetDatabase.CreateAsset(mat, MatPath);
        }
        mat.SetTexture("_BaseMap", tex);
        mat.SetColor("_BaseColor", Color.white);
        mat.SetFloat("_Surface", 1);   // Transparent
        mat.SetFloat("_Blend", 0);     // Alpha
        mat.SetFloat("_Cull", (float)CullMode.Off);
        mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.SetTextureScale("_BaseMap", Vector2.one); // AR plane UVs are in metres -> 1 tile per metre
        EditorUtility.SetDirty(mat);

        // 4. Custom AR plane prefab (replaces the default visualizer).
        var go = new GameObject("CustomARPlane");
        go.AddComponent<ARPlane>();
        go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
        go.AddComponent<MeshCollider>();
        go.AddComponent<ARPlaneMeshVisualizer>();
        PrefabUtility.SaveAsPrefabAsset(go, PrefabPath);
        Object.DestroyImmediate(go);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

        // 5. Open our scene and wire the prefab into the AR Plane Manager (horizontal only).
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var pm = Object.FindAnyObjectByType<ARPlaneManager>(FindObjectsInactive.Include);
        if (pm == null) { Debug.LogError("[ARShooterSetup] No ARPlaneManager found in scene."); return; }
        Undo.RecordObject(pm, "Assign custom plane");
        pm.planePrefab = prefab;
        pm.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        EditorUtility.SetDirty(pm);
        PrefabUtility.RecordPrefabInstancePropertyModifications(pm);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        // 6. Make ARGame the only scene in the build.
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };

        AssetDatabase.SaveAssets();
        Debug.Log("[ARShooterSetup] Step 2.2 done: ARGame scene, M_CustomPlane material, CustomARPlane prefab assigned to ARPlaneManager (Horizontal).");
        Selection.activeObject = prefab;
    }

    // ---------------------------------------------------------------------
    // Step 2.3 - Arena prefab (simple primitives, muted style)
    // ---------------------------------------------------------------------
    const string ArenaPath = Root + "/Prefabs/Arena.prefab";

    [MenuItem("Tools/AR Shooter/Step 2.3 - Arena Prefab")]
    public static void BuildArena()
    {
        EnsureFolders("Materials", "Prefabs");
        const float radius = 0.75f;          // 1.5 m wide platform

        var floorMat  = MakeLitMat("M_ArenaFloor",  new Color(0.16f, 0.17f, 0.19f), 0.35f);
        var rimMat    = MakeLitMat("M_ArenaRim",    new Color(0.42f, 0.45f, 0.48f), 0.5f);
        var propMat   = MakeLitMat("M_ArenaProp",   new Color(0.24f, 0.25f, 0.27f), 0.2f);
        var spawnMat  = MakeLitMat("M_SpawnMarker", new Color(0.45f, 0.16f, 0.14f), 0.3f);

        var root = new GameObject("Arena");
        var arena = root.AddComponent<Arena>();

        // Base platform (keeps its collider so bullets can hit the floor)
        Prim(PrimitiveType.Cylinder, "Floor", root.transform,
            new Vector3(0, 0.015f, 0), new Vector3(radius * 2, 0.015f, radius * 2), floorMat, true);

        // Thin lighter rim under the edge
        Prim(PrimitiveType.Cylinder, "Rim", root.transform,
            new Vector3(0, 0.008f, 0), new Vector3(radius * 2 + 0.05f, 0.008f, radius * 2 + 0.05f), rimMat, false);

        // A few low cover blocks
        Prim(PrimitiveType.Cube, "Cover_1", root.transform, new Vector3( 0.28f, 0.07f,  0.18f), new Vector3(0.18f, 0.08f, 0.06f), propMat, true);
        Prim(PrimitiveType.Cube, "Cover_2", root.transform, new Vector3(-0.30f, 0.07f, -0.12f), new Vector3(0.06f, 0.08f, 0.18f), propMat, true);
        Prim(PrimitiveType.Cube, "Cover_3", root.transform, new Vector3( 0.02f, 0.07f, -0.35f), new Vector3(0.14f, 0.08f, 0.06f), propMat, true);

        // 4 spawn points around the edge + small marker discs
        var spawnRoot = new GameObject("SpawnPoints").transform;
        spawnRoot.SetParent(root.transform, false);
        var points = new Transform[4];
        float r = radius - 0.1f;
        for (int i = 0; i < 4; i++)
        {
            float a = (45f + 90f * i) * Mathf.Deg2Rad;
            var sp = new GameObject("SpawnPoint_" + (i + 1)).transform;
            sp.SetParent(spawnRoot, false);
            sp.localPosition = new Vector3(Mathf.Cos(a) * r, 0.03f, Mathf.Sin(a) * r);
            points[i] = sp;
            Prim(PrimitiveType.Cylinder, "Marker", sp, new Vector3(0, 0.001f, 0), new Vector3(0.12f, 0.002f, 0.12f), spawnMat, false);
        }
        arena.EditorSetup(points, radius);

        PrefabUtility.SaveAsPrefabAsset(root, ArenaPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ArenaPath);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log("[ARShooterSetup] Step 2.3 done: Arena prefab with 4 spawn points at " + ArenaPath);
    }

    static GameObject Prim(PrimitiveType t, string name, Transform parent, Vector3 pos, Vector3 scale, Material m, bool keepCollider)
    {
        var g = GameObject.CreatePrimitive(t);
        g.name = name;
        g.transform.SetParent(parent, false);
        g.transform.localPosition = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        if (!keepCollider) Object.DestroyImmediate(g.GetComponent<Collider>());
        return g;
    }

    static Material MakeLitMat(string name, Color c, float smooth)
    {
        var path = Root + "/Materials/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.SetColor("_BaseColor", c);
        mat.SetFloat("_Smoothness", smooth);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void EnsureFolders(params string[] subs)
    {
        // Root is Assets itself - always exists
        foreach (var s in subs)
        {
            var parent = Root;
            foreach (var part in s.Split('/'))
            {
                var p = parent + "/" + part;
                if (!AssetDatabase.IsValidFolder(p)) AssetDatabase.CreateFolder(parent, part);
                parent = p;
            }
        }
    }
}
#endif
