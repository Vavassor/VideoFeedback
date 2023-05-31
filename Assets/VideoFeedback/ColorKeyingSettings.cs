
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ColorKeyingSettings : UdonSharpBehaviour
{
    public ColorButton keyColorButton;
    public Toggle isChromaKeyEnabled;
    public Toggle isLumaKeyEnabled;
    public Slider similaritySlider;
    public Slider smoothnessSlider;
    public Slider spillSlider;
    public Slider luminanceMaxSlider;
    public Slider luminanceMaxSmoothnessSlider;
    public Slider luminanceMinSlider;
    public Slider luminanceMinSmoothnessSlider;
    public Material chromaKeyMaterial;
    public Material lumaKeyMaterial;
    public CameraController cameraController;
    public CanvasGroup chromaKeyCanvasGroup;
    public CanvasGroup lumaKeyCanvasGroup;

    public void OnChangeKeyToggle()
    {
        SetCanvasGroupVisibility(chromaKeyCanvasGroup, isChromaKeyEnabled.isOn);
        SetCanvasGroupVisibility(lumaKeyCanvasGroup, isLumaKeyEnabled.isOn);
        cameraController.OnChangeKeyToggle();
    }

    public void OnChangeKeyColor()
    {
        chromaKeyMaterial.SetColor("_ChromaKey", keyColorButton.color);
    }

    public void OnChangeChromaSimilarity()
    {
        chromaKeyMaterial.SetFloat("_Similarity", similaritySlider.value);
    }

    public void OnChangeLuminanceMax()
    {
        lumaKeyMaterial.SetFloat("_LuminanceMax", luminanceMaxSlider.value);
    }

    public void OnChangeLuminanceMaxSmoothness()
    {
        lumaKeyMaterial.SetFloat("_MaxSmoothness", luminanceMaxSmoothnessSlider.value);
    }

    public void OnChangeLuminanceMin()
    {
        lumaKeyMaterial.SetFloat("_LuminanceMin", luminanceMinSlider.value);
    }

    public void OnChangeLuminanceMinSmoothness()
    {
        lumaKeyMaterial.SetFloat("_MinSmoothness", luminanceMinSmoothnessSlider.value);
    }

    public void OnChangeSmoothness()
    {
        chromaKeyMaterial.SetFloat("_Smoothness", smoothnessSlider.value);
    }

    public void OnChangeSpill()
    {
        chromaKeyMaterial.SetFloat("_Spill", spillSlider.value);
    }

    public void OnClickResetAllButton()
    {
        isChromaKeyEnabled.isOn = false;
        isLumaKeyEnabled.isOn = false;
        similaritySlider.value = 0.1f;
        smoothnessSlider.value = 0.08f;
        spillSlider.value = 0.1f;
        luminanceMinSlider.value = 0.0f;
        luminanceMinSmoothnessSlider.value = 0.0f;
        luminanceMaxSlider.value = 1.0f;
        luminanceMaxSmoothnessSlider.value = 0.0f;

        keyColorButton.color = new Color(0.05098039f, 0.6313726f, 0.1411765f);
        keyColorButton.OnSetValueExternally();
    }

    private void SetCanvasGroupVisibility(CanvasGroup canvasGroup, bool isOn)
    {
        if (isOn)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
