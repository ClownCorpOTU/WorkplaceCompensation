using UnityEngine;
using Unity.Cinemachine;

public class ScorePopupManager : MonoBehaviour
{
    public static ScorePopupManager Instance;

    [Header("UI")]
    public GameObject scorePopupPrefab;
    public RectTransform canvasTransform;

    [Header("Spawn Settings")]
    public Vector2 spawnArea = new Vector2(80f, 40f);

    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowScore(int amount)
    {
        amount = Mathf.Clamp(amount, 1, 2);

        //Spawn popup
        GameObject popupObj = Instantiate(scorePopupPrefab, canvasTransform);

        RectTransform rect = popupObj.GetComponent<RectTransform>();

        rect.anchoredPosition = new Vector2(
            Random.Range(-spawnArea.x, spawnArea.x),
            Random.Range(-spawnArea.y, spawnArea.y)
        );

        popupObj.GetComponent<ScorePopup>().Setup(amount);

        //Apply Cinemachine shake
        ApplyShake(amount);
    }

    void ApplyShake(int amount)
    {
        if (impulseSource == null) return;

        if (amount == 1)
        {
            CameraShakeManager.Instance.ApplyCameraShake(impulseSource, Vector3.up, 0.5f);
        }
        else if (amount == 2)
        {
            CameraShakeManager.Instance.ApplyCameraShake(impulseSource, Vector3.up, 1.2f);
        }
    }
}