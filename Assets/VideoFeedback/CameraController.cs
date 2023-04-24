
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public ColorButton clearColorButton;
    public Camera0Controller camera0Controller;
    public CustomRenderTexture videoGradientMapped;
    public RenderTexture videoGradientMapped1;
    public CustomRenderTexture videoMixed;
    public SyncedToggle orthographicProjectionToggle;
    public Material backboardMaterial;
    public Material videoScreenOpaqueMaterial;

    private Camera cameraComponent;

    public void OnChangeClearColor()
    {
        var color = clearColorButton.color;
        var clearColor = new Color(color.r, color.g, color.b, 0.0f);
        videoScreenOpaqueMaterial.SetColor("_BackgroundColor", clearColor);
        backboardMaterial.SetColor("_Color", clearColor);
        cameraComponent.backgroundColor = clearColor;
    }

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
