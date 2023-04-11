
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class RespawnObject : UdonSharpBehaviour
{
    private VRCObjectSync objectSync;

    void Start()
    {
        objectSync = GetComponent<VRCObjectSync>();
    }

    public void OnRespawn()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, "OnRespawnNetwork");
    }

    public void OnRespawnNetwork()
    {
        objectSync.Respawn();
    }
}
