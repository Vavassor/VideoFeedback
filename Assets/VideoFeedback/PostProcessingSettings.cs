
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PostProcessingSettings : UdonSharpBehaviour
{
    public Slider ambientOcclusionIntensitySlider;
    public PostProcessVolume ambientOcclusionVolume;
    public Slider bloomIntensitySlider;
    public PostProcessVolume bloomVolume;
    public CanvasGroup postProcessCanvasGroup;
    public Toggle postProcessingToggle;

    private PostProcessVolume postProcessVolume;

    void Start()
    {
        postProcessVolume = GetComponent<PostProcessVolume>();

        bloomIntensitySlider.SetValueWithoutNotify(bloomVolume.weight);
        ambientOcclusionIntensitySlider.SetValueWithoutNotify(ambientOcclusionVolume.weight);
    }

    public void OnChangeAmbientOcclusionIntensity()
    {
        ambientOcclusionVolume.weight = ambientOcclusionIntensitySlider.value;
    }

    public void OnChangeBloomIntensity()
    {
        bloomVolume.weight = bloomIntensitySlider.value;
    }

    public void OnChangePostProcessing()
    {
        var isEnabled = postProcessingToggle.isOn;

        postProcessVolume.enabled = isEnabled;
        bloomVolume.enabled = isEnabled;
        ambientOcclusionVolume.enabled = isEnabled;

        if (isEnabled)
        {
            postProcessCanvasGroup.alpha = 1.0f;
            postProcessCanvasGroup.blocksRaycasts = true;
            postProcessCanvasGroup.interactable = true;
        }
        else
        {
            postProcessCanvasGroup.alpha = 0.1f;
            postProcessCanvasGroup.blocksRaycasts = false;
            postProcessCanvasGroup.interactable = false;
        }
    }
}
