using TMPro;
using UnityEngine;

public class FloatingDamageNumber : MonoBehaviour
{
    [SerializeField]
    private TMP_Text damageText;

    [SerializeField]
    private float lifetime = 0.8f;

    [SerializeField]
    private float riseSpeed = 1.5f;

    [SerializeField]
    private float fadeSpeed = 2f;

    private float timer;
    private Color originalColor;

    [SerializeField] private Camera viewCamera;

    private void Awake()
    {
        if (damageText != null)
            originalColor = damageText.color;
    }

    public void Initialize(float damage)
    {
        if (damageText != null)
        {
            damageText.text = Mathf.RoundToInt(damage).ToString();
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position +=
            Vector3.up * riseSpeed * Time.deltaTime;

        if (damageText != null)
        {
            Color color = originalColor;
            color.a = Mathf.Clamp01(
                1f - timer / lifetime
            );

            damageText.color = color;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }

    private void LateUpdate()
    {
        if (viewCamera == null)
            viewCamera = Camera.main;

        if (viewCamera != null)
            transform.rotation = viewCamera.transform.rotation;
    }

    public void SetCamera(Camera camera)
    {
        viewCamera = camera;
    }
}