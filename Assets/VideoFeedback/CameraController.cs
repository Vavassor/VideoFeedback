
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public SyncedToggle cameraClearToggle;
    public CustomRenderTexture videoMixed;
    public SyncedToggle orthographicProjectionToggle;
    public RenderTexture video0;
    public RenderTexture video1;

    private Camera cameraComponent;

    public void OnChangeCameraClear()
    {
        cameraComponent.clearFlags = cameraClearToggle.isOn ? CameraClearFlags.SolidColor : CameraClearFlags.Nothing;
    }

    public void OnChangeOrthographicProjection()
    {
        cameraComponent.orthographic = orthographicProjectionToggle.isOn;
    }

    void Start()
    {
        cameraComponent = GetComponent<Camera>();

        videoMixed.Initialize();
    }

    void Update()
    {
        VRCGraphics.Blit(video0, video1);
        videoMixed.Update();
    }
}
