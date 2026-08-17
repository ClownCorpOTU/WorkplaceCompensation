using System;
using System.Collections.Generic;
using UnityEngine;

public static class GameEventManager
{
    // Dictionary mapping a GameEvent to a listening function
    private static readonly Dictionary<GameEvent, Action> eventDictionary = new();

    public static void StartListening(GameEvent eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            // Add listener to existing event
            thisEvent += listener;
            eventDictionary[eventName] = thisEvent;
        }
        else
        {
            // First time this event is being listened to, add it to the dictionary
            eventDictionary.Add(eventName, listener);
        }
    }
    
    public static void StopListening(GameEvent eventName, Action listener)
    {
        if (eventDictionary.TryGetValue(eventName, out Action thisEvent))
        {
            // Remove the listener
            thisEvent -= listener;
            eventDictionary[eventName] = thisEvent;
        }
    }

    
    // Call this from gameplay scripts to announce that an event has happened
    public static void TriggerEvent(GameEvent eventName)
    {
        Debug.Log("Triggering Event: " + eventName);
        
        if (eventDictionary.TryGetValue(eventName, out Action thisEvent))
            thisEvent?.Invoke(); // Invoke calls all the functions currently listening to this event
    }
}