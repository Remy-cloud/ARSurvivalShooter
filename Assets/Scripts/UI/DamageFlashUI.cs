using UnityEngine;
using UnityEngine.UI;

// Red screen flash when the player is hit. Listens to PlayerHealth.Damaged.
[RequireComponent(typeof(Image))]
public class DamageFlashUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private float flashAlpha = 0.35f;
    [SerializeField] private float fadeSpeed = 2.5f;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.raycastTarget = false;
        SetAlpha(0f);
    }

    private void OnEnable() => playerHealth.Damaged += Flash;
    private void OnDisable() => playerHealth.Damaged -= Flash;

    private void Flash() => SetAlpha(flashAlpha);

    private void Update()
    {
        if (image.color.a > 0f)
            SetAlpha(Mathf.MoveTowards(image.color.a, 0f, fadeSpeed * Time.deltaTime));
    }

    private void SetAlpha(float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }
}
