using UnityEngine;

/// <summary>
/// Moves the crosshair dot to wherever the gun barrel is pointing.
/// Every frame it traces a line out of the Muzzle (along the barrel) and puts the dot
/// on the first thing it hits - or on a point far ahead if it hits nothing.
/// So: dot on the enemy = bullet will hit the enemy.
/// Put this on the Crosshair UI Image.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CrosshairUI : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float defaultDistance = 3f;   // where the dot sits when the barrel points at nothing
    [SerializeField] private LayerMask aimMask = ~0;

    private RectTransform rect;
    private Camera cam;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        cam = Camera.main;
    }

    private void LateUpdate()   // after the camera and gun have moved this frame
    {
        if (!muzzle) return;

        Vector3 aimPoint = Physics.Raycast(muzzle.position, muzzle.forward, out RaycastHit hit, maxDistance, aimMask, QueryTriggerInteraction.Ignore)
            ? hit.point
            : muzzle.position + muzzle.forward * defaultDistance;

        Vector3 screen = cam.WorldToScreenPoint(aimPoint);
        if (screen.z > 0f) rect.position = screen;   // only if the point is in front of the camera
    }
}
