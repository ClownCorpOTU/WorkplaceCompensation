using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [Header("Base Settings")]
    public float baseLifetime = 1.4f;
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
        // Clamp to 1–3
        score = Mathf.Clamp(score, 1, 3);

        text.text = "+" + score;

        //Random rainbow color
        Color rainbowColor = Color.HSVToRGB(Random.value, 1f, 1f);
        text.color = rainbowColor;

        //Random rotation for juice
        transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));

        switch (score)
        {
            case 1:
                text.text = "+1";
                lifetime = baseLifetime + 0.2f;
                scaleAmount = 0.3f;
                scaleFrequency = 10f;
                break;

            case 2:
                text.text = "+2!";
                lifetime = baseLifetime + 0.35f;
                scaleAmount = 0.5f;
                scaleFrequency = 14f;
                break;

            case 3:
                text.text = "+3!!";
                lifetime = baseLifetime + 0.5f;
                scaleAmount = 0.69f;  
                scaleFrequency = 18f;  
                break;
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