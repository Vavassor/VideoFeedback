
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ProjectionSettings : UdonSharpBehaviour
{
    public CameraController cameraController;
    public SyncedToggle orthographicProjectionToggle;

    private CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnChangeOrthographicProjection()
    {
        var isEnabled = !orthographicProjectionToggle.isOn;

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

        cameraController.OnChangeOrthographicProjection();
    }
}
