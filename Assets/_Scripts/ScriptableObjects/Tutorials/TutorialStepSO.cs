using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewTutorialStep", menuName = "WorkplaceComp/Tutorial Step")]
public class TutorialStepSO : ScriptableObject
{
    public string title;
    [TextArea(3, 10)] public string description;
    public VideoClip tutorialClip;
}