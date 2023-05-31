
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

public class Teleport : UdonSharpBehaviour
{
    public Transform teleportTarget;
    public VRCObjectSync objectSync;

    public void OnTeleport()
    {
        objectSync.TeleportTo(teleportTarget);
    }
}
