using UnityEngine;

[CreateAssetMenu(menuName = "WorkplaceComp/Effects/Speed Boost", order = 0)]
public class SpeedBoostEffectSO : ConsumableEffectSO
{
    public float SpeedMultiplier = 1.6f;
    public float DurationSeconds = 7f;
    
    public override void ApplyEffect(NetworkPlayer player)
    {
        player.ApplySpeedBoostEffect(SpeedMultiplier, DurationSeconds);
    }
}