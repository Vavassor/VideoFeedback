
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class ResizeMenu : UdonSharpBehaviour
{
    public void OnRespawn()
    {
        SendCustomNetworkEvent(NetworkEventTarget.All, "OnRespawnNetwork");
    }

    public void OnRespawnNetwork()
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public void OnClickResizeHalf()
    {
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    public void OnClickResize1x()
    {
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    public void OnClickResizeQuarter()
    {
        transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
    }
}
