using UnityEngine;
using Unity.Cinemachine;

public class ScorePopupManager : MonoBehaviour
{
    public static ScorePopupManager Instance;

    [Header("UI References")]
    public GameObject scorePopupPrefab;
    public RectTransform canvasTransform;

    [Header("Bottom Left Positioning")]
    public Vector2 basePosition = new Vector2(100f, 300f); // distance from bottom-left
    public Vector2 randomOffset = new Vector2(20f, 10f);   // small variation

    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void ShowScore(int amount)
    {
        //Only allow 1 or 2
        amount = Mathf.Clamp(amount, 1, 2);

        //Spawn popup
        GameObject popupObj = Instantiate(scorePopupPrefab, canvasTransform);
        RectTransform rect = popupObj.GetComponent<RectTransform>();

        //Slight randomness 
        Vector2 offset = new Vector2(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y)
        );

        rect.anchoredPosition = basePosition + offset;

        //Setup popup
        popupObj.GetComponent<ScorePopup>().Setup(amount);

        //Apply camera shake
        ApplyShake(amount);
    }

    void ApplyShake(int amount)
    {
        if (impulseSource == null) return;

        if (amount == 1)
        {
            CameraShakeManager.Instance.ApplyCameraShake(impulseSource, Vector3.up, 0.3f);
        }
        else if (amount == 2)
        {
            CameraShakeManager.Instance.ApplyCameraShake(impulseSource, Vector3.up, 0.7f);
        }
    }
}