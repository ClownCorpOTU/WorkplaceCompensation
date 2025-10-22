using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public Vector2 RawInput; // Raw 2D input for magnitude/animation
    public Vector3 MoveDirection; // Camera-relative, world-space movement direction (Normalized)
    
    public NetworkBool IsJumpPressed;
    public NetworkBool IsRevivePressed;
    public NetworkBool IsGrabPressed, IsLeftGrabPressed, IsRightGrabPressed, IsLiftPressed;
}