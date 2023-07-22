
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PostProcessSetting : UdonSharpBehaviour
{
    public PostProcessVolume[] postProcessVolumes;
    public Slider[] sliders;
    public float value;
    public bool useTwoWayMode = true;

    public void SetIsVolumeEnabled(bool isEnabled)
    {
        foreach (var volume in postProcessVolumes)
        {
            volume.enabled = isEnabled;
        }
    }

    public void OnChangeValue(Slider slider)
    {
        if (slider.value == value)
        {
            return;
        }

        value = slider.value;

        if (postProcessVolumes.Length > 1 && useTwoWayMode)
        {
            if (value <= 0)
            {
                postProcessVolumes[0].weight = Mathf.Abs(value);
                postProcessVolumes[1].weight = 0;
            }
            else
            {
                postProcessVolumes[0].weight = 0;
                postProcessVolumes[1].weight = value;
            }
        }
        else
        {
            foreach(var volume in postProcessVolumes)
            {
                volume.weight = value;
            }
        }

        foreach (var otherSlider in sliders)
        {
            if (otherSlider != slider)
            {
                otherSlider.value = value;
            }
        }
    }
}
