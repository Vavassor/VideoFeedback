
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

public class LightingSettings : UdonSharpBehaviour
{
    public Light[] lights;
    public Toggle isShadowEnabledToggle;
    public Slider lightIntensitySlider;
    public Slider lightRangeSlider;

    public void OnChangeIsShadowEnabled()
    {
        foreach (var light in lights)
        {
            light.shadows = isShadowEnabledToggle.isOn ? LightShadows.Hard : LightShadows.None;
        }
    }

    public void OnChangeLightIntensity()
    {
        foreach (var light in lights)
        {
            light.intensity = lightIntensitySlider.value;
        }
    }

    public void OnChangeLightRange()
    {
        foreach (var light in lights)
        {
            light.range = lightRangeSlider.value;
        }
    }
}
