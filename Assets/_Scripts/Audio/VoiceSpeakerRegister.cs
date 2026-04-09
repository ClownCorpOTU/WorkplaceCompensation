using UnityEngine;

public class VoiceSpeakerRegister : MonoBehaviour
{
    private void OnEnable()
    {
        VoiceOcclusion.RegisterSpeaker(this);
    }

    private void OnDisable()
    {
        VoiceOcclusion.UnregisterSpeaker(this);
    }
}