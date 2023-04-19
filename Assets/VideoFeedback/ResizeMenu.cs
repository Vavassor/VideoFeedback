
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ResizeMenu : UdonSharpBehaviour
{
    public void OnRespawn()
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
}
