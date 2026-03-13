using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullScreenEffectsManager : MonoBehaviour
{
    public static FullScreenEffectsManager Instance;

    [SerializeField] private UniversalRendererData rendererData;
    [SerializeField] private string impactFrameFeatureName = "FS_ImpactFrame";

    private ScriptableRendererFeature impactFeature;
    private Coroutine impactFlashCoroutine;
    private Coroutine timeStopCoroutine;

    private void Awake()
    {
        Instance = this;
        impactFeature = rendererData.rendererFeatures.Find(f => f.name == impactFrameFeatureName);
        impactFeature.SetActive(false);
    }

    public void TriggerImpactFlash(int frameCount)
    {
        if (impactFlashCoroutine != null) StopCoroutine(impactFlashCoroutine);
        impactFlashCoroutine = StartCoroutine(FlashRoutine(frameCount));
    }

    public void TriggerTimeStop(float duration)
    {
        if (timeStopCoroutine != null) StopCoroutine(timeStopCoroutine);
        timeStopCoroutine = StartCoroutine(TimeStopRoutine(duration));
    }

    private IEnumerator FlashRoutine(int frameCount)
    {
        impactFeature.SetActive(true);

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        
        impactFeature.SetActive(false);
        impactFlashCoroutine = null;
    }

    private IEnumerator TimeStopRoutine(float duration)
    {
        Time.timeScale = 0.01f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1.0f;
        timeStopCoroutine = null;
    }
}