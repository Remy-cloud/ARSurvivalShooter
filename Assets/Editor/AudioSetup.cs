using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Creates the AudioManager object and fills in any clips it finds in Assets/Audio by name.
public static class AudioSetup
{
    [MenuItem("Tools/AR Shooter/Step 6 - Audio Manager")]
    public static void Build()
    {
        GameObject go = GameObject.Find("AudioManager") ?? new GameObject("AudioManager");
        AudioManager manager = go.GetComponent<AudioManager>();
        if (!manager) manager = go.AddComponent<AudioManager>();

        AudioSource[] sources = go.GetComponents<AudioSource>();
        AudioSource sfx = sources.Length > 0 ? sources[0] : go.AddComponent<AudioSource>();
        AudioSource musicSrc = sources.Length > 1 ? sources[1] : go.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        musicSrc.playOnAwake = false;
        musicSrc.loop = true;

        var so = new SerializedObject(manager);
        so.FindProperty("sfxSource").objectReferenceValue = sfx;
        so.FindProperty("musicSource").objectReferenceValue = musicSrc;
        so.FindProperty("playerHealth").objectReferenceValue = Object.FindAnyObjectByType<PlayerHealth>();

        Assign(so, "playerShoot", "laserSmall");
        Assign(so, "enemyShoot", "laserRetro");
        Assign(so, "enemySpawn", "forceField");
        Assign(so, "meleeAttack", "slime");
        Assign(so, "playerDeath", "explosion");
        AssignFirstIn(so, "music", "Assets/Audio/Music");

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(go.scene);
        Debug.Log("Step 6: AudioManager ready. Check the clip slots, then save the scene (Cmd+S).");
    }

    static void Assign(SerializedObject so, string field, string nameContains)
    {
        if (so.FindProperty(field).objectReferenceValue || !AssetDatabase.IsValidFolder("Assets/Audio")) return;
        string guid = AssetDatabase.FindAssets($"{nameContains} t:AudioClip", new[] { "Assets/Audio" }).OrderBy(g => AssetDatabase.GUIDToAssetPath(g)).FirstOrDefault();
        if (guid != null) so.FindProperty(field).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
    }

    static void AssignFirstIn(SerializedObject so, string field, string folder)
    {
        if (so.FindProperty(field).objectReferenceValue || !AssetDatabase.IsValidFolder(folder)) return;
        string guid = AssetDatabase.FindAssets("t:AudioClip", new[] { folder }).FirstOrDefault();
        if (guid != null) so.FindProperty(field).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));
    }
}
