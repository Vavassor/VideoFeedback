
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class DesktopOnly : UdonSharpBehaviour
{
    private CanvasGroup canvasGroup;

    void Start()
    {
        if(Networking.LocalPlayer.IsUserInVR() == true)
        {
            canvasGroup.alpha = 0.2f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
