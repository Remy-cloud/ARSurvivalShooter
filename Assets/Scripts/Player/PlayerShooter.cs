using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// Tap anywhere to shoot. The bullet leaves the gun's Muzzle and flies straight
/// along the barrel (the CrosshairUI dot shows where that is).
/// Bullets come from the ProjectilePool (no Instantiate/Destroy while playing).
/// Put this on the GunHolder.
/// </summary>
public class PlayerShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProjectilePool bulletPool;
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform gunModel;          // the Pistol, used for the recoil kick
    [SerializeField] private GameObject muzzleFlash;      // optional: small glow at the muzzle

    [Header("Shooting")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float fireCooldown = 0.2f;   // seconds between shots
    [SerializeField] private LayerMask hitMask = ~0;      // what player bullets can hit

    [Header("Recoil")]
    [SerializeField] private float recoilBack = 0.015f;   // metres the gun jumps back
    [SerializeField] private float recoilAngle = 8f;      // degrees the gun tips up
    [SerializeField] private float recoilRecover = 12f;   // how fast it returns

    [Header("State")]
    [SerializeField] private bool canShoot = false;       // GameManager turns this on during play
    [Tooltip("Temporary (until the GameManager exists): allow shooting as soon as the arena is placed.")]
    [SerializeField] private TapToPlaceArena enableWhenArenaPlaced;

    /// <summary>Raised every time a bullet is fired (AudioManager listens to play the shot sound).</summary>
    public static event Action Shot;

    private float nextShotTime;
    private Vector3 gunRestPos;
    private Quaternion gunRestRot;
    private float flashTimer;

    public bool CanShoot { get => canShoot; set => canShoot = value; }

    private void Awake()
    {
        if (gunModel)
        {
            gunRestPos = gunModel.localPosition;
            gunRestRot = gunModel.localRotation;
        }
        if (muzzleFlash) muzzleFlash.SetActive(false);
        if (enableWhenArenaPlaced) enableWhenArenaPlaced.ArenaPlaced += OnArenaPlaced;
    }

    private void OnDestroy()
    {
        if (enableWhenArenaPlaced) enableWhenArenaPlaced.ArenaPlaced -= OnArenaPlaced;
    }

    // Wait one frame so the tap that placed the arena doesn't also fire a bullet.
    private void OnArenaPlaced(Arena arena)
    {
        canShoot = true;
        nextShotTime = Time.time + 0.3f;   // short delay so the placing tap isn't also a shot
    }

    private void Update()
    {
        RecoverRecoil();
        UpdateFlash();

        if (!canShoot || Time.time < nextShotTime) return;
        if (TapStartedThisFrame()) Fire();
    }

    private void Fire()
    {
        // The bullet leaves the mouth of the gun and flies exactly where the barrel points.
        // The CrosshairUI shows that same line on screen, so "dot on target" = hit.
        Vector3 direction = muzzle.forward;

        if (!bulletPool.Fire(muzzle.position, direction, damage, hitMask)) return;   // pool empty: no shot

        nextShotTime = Time.time + fireCooldown;
        KickRecoil();
        if (muzzleFlash) { muzzleFlash.SetActive(true); flashTimer = 0.05f; }
        Shot?.Invoke();
    }

    private void KickRecoil()
    {
        if (!gunModel) return;
        gunModel.localPosition = gunRestPos - Vector3.forward * recoilBack;
        gunModel.localRotation = gunRestRot * Quaternion.Euler(recoilAngle, 0f, 0f);
    }

    private void RecoverRecoil()
    {
        if (!gunModel) return;
        float t = recoilRecover * Time.deltaTime;
        gunModel.localPosition = Vector3.Lerp(gunModel.localPosition, gunRestPos, t);
        gunModel.localRotation = Quaternion.Slerp(gunModel.localRotation, gunRestRot, t);
    }

    private void UpdateFlash()
    {
        if (!muzzleFlash || !muzzleFlash.activeSelf) return;
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f) muzzleFlash.SetActive(false);
    }

    private static bool TapStartedThisFrame()
    {
        // Phone
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            int id = Touchscreen.current.primaryTouch.touchId.ReadValue();
            return !IsOverUI(id);
        }
        // Editor
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return !IsOverUI(-1);
        return false;
    }

    // Taps on buttons (pause, menu...) must not also fire a bullet.
    private static bool IsOverUI(int pointerId)
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
    }
}
