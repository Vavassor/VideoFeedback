
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Screen : UdonSharpBehaviour
{
    public Material videoMixerMaterial;
    public Material gradientMappingMaterial;
    public SyncedSlider brightnessSlider;
    public SyncedSlider chromaticDistortionSlider;
    public SyncedToggle gradientMappingToggle;
    public SyncedSlider hueShiftSlider;
    public SyncedToggle invertColorToggle;
    public SyncedToggle mirrorXToggle;
    public SyncedToggle mirrorYToggle;
    public float screenScaleX = 4.0f;
    public float screenScaleY = 2.25f;

    private float scaleX;
    private float scaleY;

    void Start()
    {
        scaleX = screenScaleX;
        scaleY = screenScaleY;
    }

    public void OnChangeBrightness()
    {
        videoMixerMaterial.SetFloat("_Brightness", brightnessSlider.value);
    }

    public void OnChangeChromaticDistortion()
    {
        videoMixerMaterial.SetFloat("_ChromaticDistortion", chromaticDistortionSlider.value);
    }

    public void OnChangeGradientMapping()
    {
        gradientMappingMaterial.SetFloat("_UseGradientMapping", gradientMappingToggle.isOn ? 1.0f : 0.0f);
    }

    public void OnChangeHueShift()
    {
        videoMixerMaterial.SetFloat("_HueShift", hueShiftSlider.value);
    }

    public void OnChangeInvertColor()
    {
        videoMixerMaterial.SetFloat("_InvertColor", invertColorToggle.isOn ? 1.0f : 0.0f);
    }

    public void OnChangeMirrorX()
    {
        scaleX = mirrorXToggle.isOn ? -screenScaleX : screenScaleX;
        UpdateScale();
    }

    public void OnChangeMirrorY()
    {
        scaleY = mirrorYToggle.isOn ? -screenScaleY : screenScaleY;
        UpdateScale();
    }

    private void UpdateScale()
    {
        transform.localScale = new Vector3(scaleX, scaleY, 1.0f);
    }
}
