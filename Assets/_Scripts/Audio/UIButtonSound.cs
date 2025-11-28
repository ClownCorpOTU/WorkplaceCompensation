using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public string hoverSound = "UI_Hover";
    public string clickSound = "UI_Click";

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.instance.Play(hoverSound);
        print("Test");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.instance.Play(clickSound);
    }
}