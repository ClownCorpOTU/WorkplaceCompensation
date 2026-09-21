using Fusion;
using UnityEngine;

public struct PlayerCustomizationData : INetworkStruct
{
    public NetworkString<_32> PlayerName;
    public Color PlayerColor;
}