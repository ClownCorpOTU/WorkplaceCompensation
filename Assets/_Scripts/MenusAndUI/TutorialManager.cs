using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    [Header("Data Source")] 
    [SerializeField] private TutorialStepSO[] steps;

    [Header("UI References")] 
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Navigation")] 
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private GameObject joinButtonsContainer;

    private int currentIndex;

    
    private void Start()
    {
        joinButtonsContainer.SetActive(false);
        UpdateUI();
    }

    public void NextStep()
    {
        if (currentIndex < steps.Length - 1)
        {
            currentIndex++;
            UpdateUI();
        }
    }

    public void PreviousStep()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        TutorialStepSO currentStep = steps[currentIndex];
        
        // Update text
        titleText.text = currentStep.title;
        descriptionText.text = currentStep.description;
        
        // Update video
        if (currentStep.tutorialClip != null)
        {
            videoPlayer.clip = currentStep.tutorialClip;
            videoPlayer.Play();
        }
        
        // Toggle Navigation Visibility
        prevButton.gameObject.SetActive(currentIndex > 0);
        
        // If we are at the end, swap "Next" for the "Join" buttons
        bool isAtEnd = currentIndex == steps.Length - 1;
        nextButton.gameObject.SetActive(!isAtEnd);
        joinButtonsContainer.SetActive(isAtEnd);
    }
}