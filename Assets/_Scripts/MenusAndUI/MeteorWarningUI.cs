using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

public class MeteorWarningUI : MonoBehaviour
{
    public static MeteorWarningUI Instance;

    [Header("Components")]
    [SerializeField, Tooltip("The Canvas Group used to fade the whole warning in/out.")] private CanvasGroup uiFader;
    [SerializeField, Tooltip("The RectTransform used to shake and pulse the bar.")] private RectTransform uiElement;

    [Header("Settings")]
    [SerializeField] private float pulseSpeed = 5f;
    [SerializeField] private float shakeIntensity = 2f;
    
    private bool isActive = false;
    private Vector2 originalPos;

    void Awake() {
        Instance = this;
        originalPos = uiElement.anchoredPosition;
        uiFader.alpha = 0; // Start hidden
    }

    void Update()
    {
        // Don't run logic if it's hidden and staying hidden
        if (!isActive && uiFader.alpha <= 0) return;

        // Smooth Fade In/Out
        float targetAlpha = isActive ? 1f : 0f;
        uiFader.alpha = Mathf.MoveTowards(uiFader.alpha, targetAlpha, Time.deltaTime * 3f);

        if (uiFader.alpha > 0)
        {
            // Subtle "Heartbeat" Pulse
            float pulse = 1f + (Mathf.Sin(Time.time * pulseSpeed) * 0.03f);
            uiElement.localScale = new Vector3(pulse, pulse, 1f);

            // Emergency Jitter/Shake
            Vector2 shake = Random.insideUnitCircle * shakeIntensity;
            uiElement.anchoredPosition = originalPos + shake;
        }
    }

    public void SetWarning(bool state) => isActive = state;
}