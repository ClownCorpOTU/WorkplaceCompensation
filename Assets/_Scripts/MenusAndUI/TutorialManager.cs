using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    [Header("Data Source")] 
    [SerializeField] private TutorialStepSO[] steps;

    [Header("UI Containers")]
    [SerializeField] private GameObject coverPage;
    [SerializeField] private GameObject contentPage;

    [Header("UI References")] 
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Navigation")] 
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private GameObject joinButtonsContainer;
    
    private int currentIndex = -1; // Start at -1 to represent the Cover Page

    
    private void Start()
    {
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
        // Allow going back as long as we aren't already on the cover
        if (currentIndex >= 0)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // 1. Handle Cover Page State
        if (currentIndex == -1)
        {
            coverPage.SetActive(true);
            contentPage.SetActive(false);
            joinButtonsContainer.SetActive(false);

            prevButton.gameObject.SetActive(false); // Can't go back from cover
            nextButton.gameObject.SetActive(true);  // Arrow to enter handbook
            return;
        }

        // 2. Handle Tutorial Content State
        coverPage.SetActive(false);
        contentPage.SetActive(true);

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
        
        // Navigation Logic
        // Prev button is active on Step 0 so we can go back to cover
        prevButton.gameObject.SetActive(true); 
        
        bool isAtEnd = currentIndex == steps.Length - 1;
        nextButton.gameObject.SetActive(!isAtEnd);
        joinButtonsContainer.SetActive(isAtEnd);
    }
}