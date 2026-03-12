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

    private void Awake()
    {
        Instance = this;
        impactFeature = rendererData.rendererFeatures.Find(f => f.name == impactFrameFeatureName);
        impactFeature.SetActive(false);
    }

    public void TriggerImpactFlash(int frameCount)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(frameCount));
    }

    private IEnumerator FlashRoutine(int frameCount)
    {
        impactFeature.SetActive(true);

        for (int i = 0; i < frameCount; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        
        impactFeature.SetActive(false);
    }
}