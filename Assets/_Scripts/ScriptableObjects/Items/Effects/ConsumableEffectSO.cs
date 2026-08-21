using UnityEngine;

// This is an abstract SO, which means we can create specific effects from this base class
public abstract class ConsumableEffectSO : ScriptableObject
{
    public abstract void ApplyEffect(NetworkPlayer player);
}