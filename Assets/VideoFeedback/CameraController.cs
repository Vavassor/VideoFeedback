
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public SyncedToggle cameraClearToggle;
    public ColorButton clearColorButton;
    public Camera0Controller camera0Controller;
    public SyncedSlider fieldOfViewSlider;
    public CustomRenderTexture videoGradientMapped;
    public RenderTexture videoGradientMapped1;
    public CustomRenderTexture videoMixed;
    public SyncedToggle orthographicProjectionToggle;
    public Material backboardMaterial;
    public Material gradientMappingMaterial;
    public Material videoScreenOpaqueMaterial;

    private Camera cameraComponent;

    public void OnChangeCameraClear()
    {
        var isClearEnabled = cameraClearToggle.isOn;
        gradientMappingMaterial.SetFloat("_ShouldClearColor", isClearEnabled ? 1.0f : 0.0f);
        var transparentBlack = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        SetClearColor(isClearEnabled ? clearColorButton.color : transparentBlack);
    }

    public void OnChangeClearColor()
    {
        if (cameraClearToggle.isOn)
        {
            SetClearColor(clearColorButton.color);
        }
    }

    public void OnChangeFieldOfView()
    {
        cameraComponent.fieldOfView = fieldOfViewSlider.value;
        camera0Controller.OnChangeFieldOfView();
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

    private void SetClearColor(Color color)
    {
        var opaqueClearColor = new Color(color.r, color.g, color.b, 1.0f);
        videoScreenOpaqueMaterial.SetColor("_BackgroundColor", opaqueClearColor);
        backboardMaterial.SetColor("_Color", opaqueClearColor);
        cameraComponent.backgroundColor = color;
    }
}
