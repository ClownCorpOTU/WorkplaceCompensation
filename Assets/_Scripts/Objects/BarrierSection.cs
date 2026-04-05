using UnityEngine;

public class BarrierSection : MonoBehaviour
{
    public Transform player;
    public float revealDistance = 5f;
    public float fadeSpeed = 5f;

    private Renderer rend;
    private Material mat;
    private float currentAlpha = 0f;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        mat = rend.material;

        SetAlpha(0f); // start invisible
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);

        float targetAlpha = distance < revealDistance ? 1f : 0f;

        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        SetAlpha(currentAlpha);
    }

    void SetAlpha(float alpha)
    {
        Color color = mat.color;
        color.a = alpha;
        mat.color = color;
    }
}