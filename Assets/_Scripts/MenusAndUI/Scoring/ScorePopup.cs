using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [Header("Base Settings")]
    public float baseLifetime = 1f;
    public float floatSpeed = 50f;

    private TextMeshProUGUI text;
    private float timer;
    private float lifetime;
    private float scaleAmount;
    private float scaleFrequency;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(int score)
    {
        text.text = "+" + score;

        // Random rotation for juice
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));

        // Different feel for +1 vs +2
        if (score == 1)
        {
            text.color = Color.white;
            lifetime = baseLifetime;
            scaleAmount = 0.4f;
            scaleFrequency = 10f;
        }
        else if (score == 2)
        {
            text.color = Color.yellow;
            lifetime = baseLifetime + 0.2f;
            scaleAmount = 0.69f;      // bigger pop
            scaleFrequency = 14f;    // snappier
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Move upward
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // Scale animation
        float scale = 1 + Mathf.Sin(timer * scaleFrequency) * scaleAmount;
        transform.localScale = Vector3.one * scale;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        text.alpha = alpha;

        // Destroy when done
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}