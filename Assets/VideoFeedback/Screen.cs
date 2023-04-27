﻿
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Screen : UdonSharpBehaviour
{
    public Material gradientMappingMaterial;
    public Material videoMixerMaterial;
    public ColorButton gradientStop0ColorButton;
    public ColorButton gradientStop1ColorButton;
    public SyncedSlider brightnessSlider;
    public SyncedSlider chromaticDistortionSlider;
    public SyncedSlider edgeBrightnessSlider;
    public SyncedSlider flowDistortionSlider;
    public SyncedToggle gradientMappingToggle;
    public SyncedSlider hueShiftSlider;
    public SyncedToggle invertColorToggle;
    public SyncedSlider mirrorTileCountSlider;
    public SyncedToggle mirrorXToggle;
    public SyncedToggle mirrorYToggle;
    public SyncedSlider sharpnessSlider;
    public float screenScaleX = 4.0f;
    public float screenScaleY = 2.25f;
    public Camera videoCamera1;

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

    public void OnChangeEdgeBrightness()
    {
        videoMixerMaterial.SetFloat("_EdgeBrightness", edgeBrightnessSlider.value);
    }

    public void OnChangeFlowDistortion()
    {
        videoMixerMaterial.SetFloat("_FlowDistortion", flowDistortionSlider.value);
    }

    public void OnChangeGradientMapping()
    {
        gradientMappingMaterial.SetFloat("_UseGradientMapping", gradientMappingToggle.isOn ? 1.0f : 0.0f);
    }

    public void OnChangeGradientStop0Color()
    {
        gradientMappingMaterial.SetColor("_GradientStop0Color", gradientStop0ColorButton.color);
    }

    public void OnChangeGradientStop1Color()
    {
        gradientMappingMaterial.SetColor("_GradientStop1Color", gradientStop1ColorButton.color);
    }

    public void OnChangeHueShift()
    {
        videoMixerMaterial.SetFloat("_HueShift", hueShiftSlider.value);
    }

    public void OnChangeInvertColor()
    {
        videoMixerMaterial.SetFloat("_InvertColor", invertColorToggle.isOn ? 1.0f : 0.0f);
    }

    public void OnChangeMirrorTileCount()
    {
        videoMixerMaterial.SetFloat("_MirrorTileCount", mirrorTileCountSlider.value);
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

    public void OnChangeSharpness()
    {
        videoMixerMaterial.SetFloat("_SharpenAmount", sharpnessSlider.value);
    }

    private void UpdateScale()
    {
        transform.localScale = new Vector3(scaleX, scaleY, 1.0f);
    }
}
