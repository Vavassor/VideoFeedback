
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public SyncedToggle cameraClearToggle;
    public ColorButton clearColorButton;
    public Camera0Controller camera0Controller;
    public SyncedSlider fieldOfViewSlider;
    public SyncedToggle isChromaKeyEnabledToggle;
    public SyncedToggle isLumaKeyEnabledToggle;
    public Toggle isStabilizerEnabledToggle;
    public RenderTexture video0Texture;
    public CustomRenderTexture videoColorKeyed;
    public CustomRenderTexture videoGradientMapped;
    public RenderTexture videoGradientMapped1;
    public CustomRenderTexture videoMixed;
    public SyncedToggle orthographicProjectionToggle;
    public Material backboardMaterial;
    public Material chromaKeyMaterial;
    public Material gradientMappingMaterial;
    public Material lumaKeyMaterial;
    public Material videoScreenOpaqueMaterial;
    public Stabilizer stabilizer;

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

    public void OnChangeKeyToggle()
    {
        if (isChromaKeyEnabledToggle.isOn)
        {
            videoColorKeyed.material = chromaKeyMaterial;
            gradientMappingMaterial.SetTexture("_Video0Texture", videoColorKeyed);
        }
        else if(isLumaKeyEnabledToggle.isOn)
        {
            videoColorKeyed.material = lumaKeyMaterial;
            gradientMappingMaterial.SetTexture("_Video0Texture", videoColorKeyed);
        }
        else
        {
            gradientMappingMaterial.SetTexture("_Video0Texture", video0Texture);
        }
    }

    public void OnChangeOrthographicProjection()
    {
        cameraComponent.orthographic = orthographicProjectionToggle.isOn;
        camera0Controller.OnChangeOrthographicProjection();
    }

    public void OnChangeIsStablizerEnabled()
    {
        stabilizer.IsStabilizing = isStabilizerEnabledToggle.isOn;
    }

    void Start()
    {
        cameraComponent = GetComponent<Camera>();

        videoColorKeyed.Initialize();
        videoGradientMapped.Initialize();
        videoMixed.Initialize();
    }

    void Update()
    {
        if (isChromaKeyEnabledToggle.isOn || isLumaKeyEnabledToggle.isOn)
        {
            videoColorKeyed.Update();
        }
            
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
