using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameplayTutorialManager : MonoBehaviour
{
    [Header("Tutorial Data")]
    [SerializeField] private GameplayTutorialStepSO[] tutorialSteps;
    
    [Header("UI Data")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private VideoPlayer videoPlayer;

    private int currentStepIndex = 0;

    private void Start()
    {
        if (tutorialSteps.Length > 0)
        {
            currentStepIndex = 0;
            tutorialPanel.SetActive(true);
            UpdateUI();
            SubscribeToCurrentStepEvent();
        }
        else
        {
            tutorialPanel.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        GameplayTutorialStepSO currentStep = tutorialSteps[currentStepIndex];
        
        titleText.text = currentStep.title;
        descriptionText.text = currentStep.description;
        if (currentStep.tutorialClip != null)
        {
            videoPlayer.clip = currentStep.tutorialClip;
            //videoPlayer.Play();
        }
    }

    private void SubscribeToCurrentStepEvent()
    {
        // Tell the EventManager "When the event for my current step happens, call my OnStepCompleted function"
        GameEvent eventToWaitFor = tutorialSteps[currentStepIndex].completionEvent;
        GameEventManager.StartListening(eventToWaitFor, OnStepCompleted);
    }
    
    private void UnsubscribeFromCurrentStepEvent()
    {
        GameEvent eventWeWereWaitingFor = tutorialSteps[currentStepIndex].completionEvent;
        GameEventManager.StopListening(eventWeWereWaitingFor, OnStepCompleted);
    }

    private void OnStepCompleted()
    {
        // Unsubscribe from the event that just fired
        UnsubscribeFromCurrentStepEvent();
        // Move to the next step
        currentStepIndex++;

        // Check if we finished the whole tutorial
        if (currentStepIndex >= tutorialSteps.Length)
            FinishTutorial();
        // If not, repeat everything for the next step
        else
        {
            UpdateUI();
            SubscribeToCurrentStepEvent();
        }
    }

    private void FinishTutorial()
    {
        Debug.Log("Gameplay Tutorial Finished!");
        tutorialPanel.SetActive(false);
    }
    
    
    private void OnDestroy()
    {
        // Failsafe: if this object is destroyed (e.g. scene changes), make sure we aren't still listening
        if (tutorialSteps != null && currentStepIndex < tutorialSteps.Length)
            UnsubscribeFromCurrentStepEvent();
    }
}