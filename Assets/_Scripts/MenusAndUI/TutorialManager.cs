using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [FormerlySerializedAs("steps")]
    [Header("Data Source")] 
    [SerializeField] private TutorialStepSO[] level1Steps;
    [SerializeField] private TutorialStepSO[] level2Steps;
    [SerializeField] private string level1Name, level2Name;

    [Header("UI Containers")]
    [SerializeField] private GameObject coverPage;
    [SerializeField] private GameObject contentPage;
    [SerializeField] private GameObject closingPage;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject tutorialMenu; 

    [Header("UI References")] 
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RawImage displayImage;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Navigation")] 
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;

    private TutorialStepSO[] steps;
    private int currentIndex = -1; // Start at -1 to represent the Cover Page
    private string chosenLevel;
    
    
    private void Awake() {
        if (Instance == null) 
            Instance = this;
        else
            Destroy(gameObject);
    }
    
    private void Start() => UpdateUI();

    private void OnEnable()
    {
        currentIndex = -1;
        UpdateUI();
    }

    public void SetLevel(string levelName)
    {
        chosenLevel = levelName;
        
        if (chosenLevel.Equals(level1Name)) steps = level1Steps;
        else if (chosenLevel.Equals(level2Name)) steps = level2Steps;
        else Debug.LogError("INVALID LEVEL NAME!");
        
        currentIndex = -1;
        UpdateUI();
    }
    
    // Will be changed later to use Jeff's lobby system
    public void JonGameButton()
    {
        SceneManager.LoadScene(chosenLevel);
    }

    public void NextStep()
    {
        if (steps != null && currentIndex < steps.Length)
        {
            currentIndex++;
            UpdateUI();
        }
    }

    public void PreviousStep()
    {
        // Allow going back as long as we aren't already on the cover
        if (currentIndex > -1)
        {
            currentIndex--;
            UpdateUI();
        }
    }

    public void CloseTutorialMenu()
    {
        tutorialMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void SkipTutorial()
    {
        if (steps == null) return;
        
        currentIndex = steps.Length;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (steps == null) return;
        
        // 1. Handle Cover Page State
        if (currentIndex == -1)
        {
            SetPageState(cover: true, content: false, closing: false);
            prevButton.gameObject.SetActive(false);
            nextButton.gameObject.SetActive(true);
            return;
        }
        
        // 2. Closing Page State (Index is equal to Length)
        if (currentIndex == steps.Length)
        {
            SetPageState(cover: false, content: false, closing: true);
            prevButton.gameObject.SetActive(true);
            nextButton.gameObject.SetActive(false);
            return;
        }
        
        // 3. Tutorial Content State (Standard Step)
        SetPageState(cover: false, content: true, closing: false);
        
        TutorialStepSO currentStep = steps[currentIndex];
        titleText.text = currentStep.title;
        descriptionText.text = currentStep.description;

        if (currentStep.tutorialClip != null)
        {
            videoPlayer.clip = currentStep.tutorialClip;
            videoPlayer.Play();
        }

        prevButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(true);
    }
    
    private void SetPageState(bool cover, bool content, bool closing)
    {
        coverPage.SetActive(cover);
        contentPage.SetActive(content);
        closingPage.SetActive(closing);
    }
}