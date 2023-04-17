
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public Camera0Controller camera0Controller;
    public CustomRenderTexture videoGradientMapped;
    public RenderTexture videoGradientMapped1;
    public CustomRenderTexture videoMixed;
    public SyncedToggle orthographicProjectionToggle;

    private Camera cameraComponent;

    public void OnChangeOrthographicProjection()
    {
        cameraComponent.orthographic = orthographicProjectionToggle.isOn;
        camera0Controller.OnChangeOrthographicProjection();
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
        VRCGraphics.Blit(videoGradientMapped, videoGradientMapped1);

        videoMixed.Update();
    }
}
