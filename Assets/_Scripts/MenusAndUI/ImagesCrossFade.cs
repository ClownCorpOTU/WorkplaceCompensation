using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImagesCrossFade : MonoBehaviour
{
    [SerializeField] Sprite[] backgroundSprites;
    [SerializeField] float displayDuration = 5.0f;
    [SerializeField] float fadeDuration = 1.2f;
    [SerializeField] bool randomize = true;

    // We need two Image layers to achieve a true crossfade. 
    // Fading a single image's alpha down and back up would reveal whatever empty space is behind it (a 'dip to black/clear').
    // By keeping the base image solid and fading an overlay image on top, the transition is completely seamless.
    private Image baseImage;
    private Image overlayImage;

    private Coroutine transitionCoroutine;
    private List<int> shuffleBag = new List<int>();
    private int lastSpriteIndex = -1;

    
    void Awake()
    {
        baseImage = GetComponent<Image>();
        CreateOverlayLayer();
    }

    void OnEnable()
    {
        if (backgroundSprites == null || backgroundSprites.Length == 0)
        {
            Debug.LogWarning("[ImagesCrossFade] No background sprites assigned in the inspector.", this);
            return;
        }

        // Start cycling whenever the menu GameObject is activated
        transitionCoroutine = StartCoroutine(CycleBackgroundsRoutine());
    }

    void OnDisable()
    {
        // Stop the coroutine if the player navigates away to prevent memory leaks or background UI updates
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
    }

    /// <summary>
    /// Spawns a hidden child Image that perfectly stretches over the base Image.
    /// This overlay handles the fading in and out of the incoming slide.
    /// </summary>
    void CreateOverlayLayer()
    {
        GameObject overlayObj = new GameObject("CrossfadeOverlay", typeof(RectTransform), typeof(Image));
        overlayObj.transform.SetParent(transform, false);

        // Match anchors and offsets to fill the exact dimensions of the parent image
        RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        overlayImage = overlayObj.GetComponent<Image>();
        overlayImage.preserveAspect = baseImage.preserveAspect;
        overlayImage.raycastTarget = false; // Don't block button clicks underneath the backdrop

        // Start overlay completely invisible
        Color transparent = baseImage.color;
        transparent.a = 0f;
        overlayImage.color = transparent;
    }

    /// <summary>
    /// Main background loop: waits, picks a slide, fades the overlay in, swaps the base image, then resets.
    /// </summary>
    IEnumerator CycleBackgroundsRoutine()
    {
        // Set the very first background image immediately without fading
        int currentIndex = GetNextSpriteIndex();
        baseImage.sprite = backgroundSprites[currentIndex];
        baseImage.color = new Color(baseImage.color.r, baseImage.color.g, baseImage.color.b, 1f);

        // If there's only 1 image, keep it on screen and don't bother running the fade loop
        if (backgroundSprites.Length <= 1) yield break;

        while (true)
        {
            // 1. Wait while the current image is presented to the player
            yield return new WaitForSecondsRealtime(displayDuration);

            // 2. Select the next image (guaranteed not to be the same as current)
            int nextIndex = GetNextSpriteIndex();
            overlayImage.sprite = backgroundSprites[nextIndex];

            // 3. Smoothly fade the overlay image alpha from 0 to 1 over fadeDuration
            float elapsed = 0f;
            Color overlayColor = overlayImage.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime; // unscaledDeltaTime ensures fades still work if Time.timeScale = 0 (e.g., paused)
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                // SmoothStep creates an ease-in/ease-out curve so the blend looks natural rather than a rigid linear fade
                overlayColor.a = Mathf.SmoothStep(0f, 1f, t);
                overlayImage.color = overlayColor;

                yield return null;
            }

            // 4. Once fully faded in, transfer the new sprite to the base image and make the overlay transparent again.
            // Because both images show the exact same sprite at this moment, the swap is visually invisible to the player.
            baseImage.sprite = overlayImage.sprite;
            overlayColor.a = 0f;
            overlayImage.color = overlayColor;
        }
    }

    /// <summary>
    /// Uses a "shuffle bag" approach: builds a deck of all sprite indices, shuffles them, 
    /// and draws one by one. This ensures true variety and prevents repetitive patterns.
    /// </summary>
    int GetNextSpriteIndex()
    {
        if (backgroundSprites.Length == 1) return 0;

        if (!randomize)
        {
            lastSpriteIndex = (lastSpriteIndex + 1) % backgroundSprites.Length;
            return lastSpriteIndex;
        }

        // Refill the deck when empty
        if (shuffleBag.Count == 0)
        {
            for (int i = 0; i < backgroundSprites.Length; i++)
            {
                // Avoid placing the current image at the very start of the next deck to prevent back-to-back repeats
                if (i != lastSpriteIndex || backgroundSprites.Length <= 1)
                {
                    shuffleBag.Add(i);
                }
            }

            // Fisher-Yates array shuffle
            for (int i = shuffleBag.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                int temp = shuffleBag[i];
                shuffleBag[i] = shuffleBag[randomIndex];
                shuffleBag[randomIndex] = temp;
            }

            // If we excluded the lastSpriteIndex above, add it back somewhere non-zero so it gets used in the new cycle
            if (shuffleBag.Count < backgroundSprites.Length && lastSpriteIndex != -1)
            {
                int insertPos = Random.Range(1, shuffleBag.Count + 1);
                shuffleBag.Insert(insertPos, lastSpriteIndex);
            }
        }

        int pickedIndex = shuffleBag[0];
        shuffleBag.RemoveAt(0);
        lastSpriteIndex = pickedIndex;

        return pickedIndex;
    }
}