using System;
using UnityEngine;

public class InstructionalSFx : MonoBehaviour
{
    [SerializeField] private NetworkButton button;
    [SerializeField] private RoomType roomType = RoomType.MainHub;
    
    private AudioManager audioManager;
    private Vector3 buttonPos;
    
    
    private void Start()
    {
        audioManager = AudioManager.instance;
        buttonPos = button.transform.position;
    }

    private void Update()
    {
        if (button == null || button.Object == null || !button.Object.IsValid)
            return;
        
        // Would be better if this function only runs when the button state changes instead of every update
        
        if (button.IsButtonPressed)
        {
            print("Button pressed");

            switch (roomType)
            {
                case RoomType.MainHub:
                    audioManager.Play("MainHubInstructions", buttonPos);
                    break;
                case RoomType.Storage:
                    audioManager.Play("StorageInstructions", buttonPos);
                    break;
                case RoomType.Processing:
                    audioManager.Play("ProcessingInstructions", buttonPos);
                    break;
                case RoomType.Mixing:
                    audioManager.Play("MixingInstructions", buttonPos);
                    break;
                case RoomType.Acid:
                    audioManager.Play("AcidInstructions", buttonPos);
                    break;
                default:
                    Debug.LogWarning("Invalid room type");
                    break;
            }
        }
    }
}

public enum RoomType
{
    MainHub,
    Storage,
    Processing,
    Mixing,
    Acid
}