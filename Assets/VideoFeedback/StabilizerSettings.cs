
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class StabilizerSettings : UdonSharpBehaviour
{
    public CameraController cameraController;
    public SyncedToggle isStabilizerEnabledToggle;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnChangeIsStablizerEnabled()
    {
        var isEnabled = isStabilizerEnabledToggle.isOn;
        
        if (isEnabled)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.alpha = 0.25f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        cameraController.OnChangeIsStablizerEnabled();
    }
}
