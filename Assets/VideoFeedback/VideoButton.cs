
using UdonSharp;
using UdonSharp.Video;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class VideoButton : UdonSharpBehaviour
{
    public USharpVideoPlayer targetVideoPlayer;
    public VRCUrl[] playlist = new VRCUrl[0];

    Button button;

    void Start()
    {
        button = GetComponentInChildren<Button>();
        UpdateOwnership();

        targetVideoPlayer.RegisterCallbackReceiver(this);
    }

    public void OnButtonPress()
    {
        targetVideoPlayer.PlayVideo(playlist[0]);
    }

    public void OnUSharpVideoLockChange()
    {
        UpdateOwnership();
    }

    public void OnUSharpVideoOwnershipChange()
    {
        UpdateOwnership();
    }

    void UpdateOwnership()
    {
        button.interactable = targetVideoPlayer.CanControlVideoPlayer();
    }
}
