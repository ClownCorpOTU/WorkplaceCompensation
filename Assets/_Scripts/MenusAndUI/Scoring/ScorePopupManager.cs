using UnityEngine;
using Unity.Cinemachine;

public class ScorePopupManager : MonoBehaviour
{
    public static ScorePopupManager Instance;

    [Header("UI References")]
    public GameObject scorePopupPrefab;
    public RectTransform canvasTransform;

    [Header("Bottom Left Positioning")]
    public Vector2 basePosition = new Vector2(100f, 300f); //distance from bottom left 
    public Vector2 randomOffset = new Vector2(20f, 10f);   //variation

    [Header("Camera Shake")]
    public CinemachineImpulseSource impulseSource;
    [Range(0f, 1f)] public float shakeMultiplier = 0.6f; 

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
        //Only allow 1 to 3
        amount = Mathf.Clamp(amount, 1, 3);

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
        
        // Play jingle
        if (AudioManager.instance != null) AudioManager.instance.Play("ScoreJingle");

        //Apply camera shake
        ApplyShake(amount);
    }
    
    private void ApplyShake(int amount)
    {
        if (impulseSource == null || CameraShakeManager.Instance == null)
            return;

        float baseForce = 0f;

        switch (amount)
        {
            case 1: baseForce = 0.5f; break;
            case 2: baseForce = 1.2f; break;
            case 3: baseForce = 2.0f; break;
        }

        float force = baseForce * shakeMultiplier;

        //Slight randomness
        Vector3 direction = new Vector3(Random.Range(-0.420f, 0.69f), Random.Range(-0.420f, 0.69f), Random.Range(-0.420f, 0.69f));

        CameraShakeManager.Instance.ApplyCameraShake(impulseSource, direction, force);
    }
}