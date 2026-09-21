using UnityEngine;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewGameplayTutorialStep", menuName = "WorkplaceComp/Gameplay Tutorial Step")]
public class GameplayTutorialStepSO : ScriptableObject
{
    public string title;
    [TextArea(3, 10)] public string description;
    public VideoClip tutorialClip;
    public GameEvent completionEvent;
}