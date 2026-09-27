using UnityEngine;

public class Arena : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float radius = 0.75f;

    public Transform[] SpawnPoints => spawnPoints;
    public float Radius => radius;

    /// <summary>Returns a random spawn point </summary>
    public Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return transform;
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

#if UNITY_EDITOR
    // Editor-only: lets the setup script fill in the spawn points.
    public void EditorSetup(Transform[] points, float r) { spawnPoints = points; radius = r; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (spawnPoints == null) return;
        foreach (var p in spawnPoints)
            if (p) Gizmos.DrawWireSphere(p.position, 0.05f);
    }
#endif
}
