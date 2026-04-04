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

        //Random rainbow color
        Color rainbowColor = Color.HSVToRGB(Random.value, 1f, 1f);
        text.color = rainbowColor;

        //Random rotation for juice
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));

        //Keep your +1 vs +2 behavior
        if (score == 1)
        {
            lifetime = baseLifetime;
            scaleAmount = 0.3f;
            scaleFrequency = 10f;
        }
        else if (score == 2)
        {
            lifetime = baseLifetime + 0.2f;
            scaleAmount = 0.6f;
            scaleFrequency = 14f;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        //Move upward
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        //Scale animation
        float scale = 1 + Mathf.Sin(timer * scaleFrequency) * scaleAmount;
        transform.localScale = Vector3.one * scale;

        //Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        text.alpha = alpha;

        //Destroy when done
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}