using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Step 2.4 + 2.5
/// Tap on a detected horizontal plane to place the Arena ONCE,
/// centred under the player (the player stands in the middle of the arena).
/// After placing: plane detection stops and the plane visuals are hidden.
/// Put this on the XR Origin (it needs the ARRaycastManager + ARPlaneManager there).
/// </summary>
[RequireComponent(typeof(ARRaycastManager))]
public class TapToPlaceArena : MonoBehaviour
{
    [Header("What to place")]
    [SerializeField] private Arena arenaPrefab;

    [Header("After placing")]
    [SerializeField] private bool stopPlaneDetection = true;

    // Other scripts (GameManager, EnemySpawner) listen to this to know when the game world exists.
    public event Action<Arena> ArenaPlaced;

    public bool IsPlaced => placedArena != null;
    public Arena PlacedArena => placedArena;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private Camera arCamera;
    private Arena placedArena;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        arCamera = Camera.main;
    }

    private void Update()
    {
        // 1. Only one arena allowed: ignore every tap after the first placement.
        if (IsPlaced) return;

        // 2. Did the player tap (or click, in the editor) this frame?
        if (!TryGetTapPosition(out Vector2 screenPos)) return;

        // 3. Shoot a ray from the tap into the real world and see if it hits a detected plane.
        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon)) return;

        ARRaycastHit hit = hits[0];

        // 4. Only accept horizontal floor-type planes (not walls or ceilings).
        if (hit.trackable is ARPlane plane && plane.alignment != PlaneAlignment.HorizontalUp) return;

        PlaceArena(hit.pose);
    }

    private void PlaceArena(Pose pose)
    {
        // Centre the arena UNDER THE PLAYER, not where they tapped:
        // take the phone's X/Z position and the tapped floor's height (Y).
        Vector3 camPos = arCamera.transform.position;
        Vector3 position = new Vector3(camPos.x, pose.position.y, camPos.z);

        // Turn the arena to match the direction the player is looking (vertical axis only).
        Vector3 forward = arCamera.transform.forward;
        forward.y = 0f;
        Quaternion rotation = forward.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(forward.normalized, Vector3.up)
            : Quaternion.identity;

        placedArena = Instantiate(arenaPrefab, position, rotation);

        // Anchor it to the real world so it stays in place as ARKit refines tracking.
        placedArena.gameObject.AddComponent<ARAnchor>();

        if (stopPlaneDetection) StopPlaneDetection();

        ArenaPlaced?.Invoke(placedArena);
    }

    // Step 2.5: stop looking for new planes and hide the ones we already found.
    private void StopPlaneDetection()
    {
        if (planeManager == null) return;
        planeManager.enabled = false;
        foreach (ARPlane p in planeManager.trackables)
            p.gameObject.SetActive(false);
    }

    private static bool TryGetTapPosition(out Vector2 position)
    {
        // Phone: first finger touch that just started.
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
        // Editor / XR Simulation: left mouse click.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }
        position = default;
        return false;
    }
}
