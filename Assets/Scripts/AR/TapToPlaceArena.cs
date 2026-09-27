using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
// Tap on a detected horizontal plane to place the Arena ONCE,
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
        if (IsPlaced) return;

        if (!TryGetTapPosition(out Vector2 screenPos)) return;

        if (!raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon)) return;

        ARRaycastHit hit = hits[0];

        if (hit.trackable is ARPlane plane && plane.alignment != PlaneAlignment.HorizontalUp) return;

        PlaceArena(hit.pose);
    }

    private void PlaceArena(Pose pose)
    {
        Vector3 camPos = arCamera.transform.position;
        Vector3 position = new Vector3(camPos.x, pose.position.y, camPos.z);

        Vector3 forward = arCamera.transform.forward;
        forward.y = 0f;
        Quaternion rotation = forward.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(forward.normalized, Vector3.up)
            : Quaternion.identity;

        placedArena = Instantiate(arenaPrefab, position, rotation);

        placedArena.gameObject.AddComponent<ARAnchor>();

        if (stopPlaneDetection) StopPlaneDetection();

        ArenaPlaced?.Invoke(placedArena);
    }

    //stop looking for new planes and hide the ones we already found.
    private void StopPlaneDetection()
    {
        if (planeManager == null) return;
        planeManager.enabled = false;
        foreach (ARPlane p in planeManager.trackables)
            p.gameObject.SetActive(false);
    }

    private static bool TryGetTapPosition(out Vector2 position)
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            position = Touchscreen.current.primaryTouch.position.ReadValue();
            return true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }
        position = default;
        return false;
    }
}
