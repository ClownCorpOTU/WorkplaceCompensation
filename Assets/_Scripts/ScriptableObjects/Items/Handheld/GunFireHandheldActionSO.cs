using UnityEngine;

[CreateAssetMenu(menuName = "WorkplaceComp/Actions/Boxing Glove Gun", order = 0)]
public class GunFireHandheldActionSO : HandheldEffectSO
{
    public override void UseHandheldItem(NetworkPlayer player)
    {
        player.GetComponentInChildren<NetworkBoxingGloveGun>().FireGlove();
    }
}