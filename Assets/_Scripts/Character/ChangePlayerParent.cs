using System;
using UnityEngine;

/// <summary>
/// Sits on any gameobjects that are moving (Like platforms) so the player can move with them.
/// </summary>
public class ChangePlayerParent : MonoBehaviour
{
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.SetParent(transform, true);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.transform.parent = null;
        }
    }
}
