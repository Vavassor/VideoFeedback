
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public SyncedToggle cameraClearToggle;
    public Camera0Controller cameraController;
    public CustomRenderTexture videoGradientMapped;
    public CustomRenderTexture videoMixed;
    public SyncedToggle orthographicProjectionToggle;

    private Camera cameraComponent;

    public void OnChangeCameraClear()
    {
        cameraComponent.clearFlags = cameraClearToggle.isOn ? CameraClearFlags.SolidColor : CameraClearFlags.Depth;
    }

    public void OnChangeOrthographicProjection()
    {
        cameraComponent.orthographic = orthographicProjectionToggle.isOn;
        cameraController.OnChangeOrthographicProjection();
    }

    void Start()
    {
        cameraComponent = GetComponent<Camera>();

        videoGradientMapped.Initialize();
        videoMixed.Initialize();
    }

    void Update()
    {
        videoGradientMapped.Update();
        videoMixed.Update();
    }
}
