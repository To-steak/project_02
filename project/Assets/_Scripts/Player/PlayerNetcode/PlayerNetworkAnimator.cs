using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
