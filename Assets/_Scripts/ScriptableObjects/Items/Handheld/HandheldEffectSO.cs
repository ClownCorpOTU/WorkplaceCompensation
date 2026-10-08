using UnityEngine;

// This is an abstract SO, which means we can create specific effects from this base class
public abstract class HandheldEffectSO : ScriptableObject
{
    public abstract void UseHandheldItem(NetworkPlayer player);
}