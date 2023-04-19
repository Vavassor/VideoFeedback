
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GradientMappingSettings : UdonSharpBehaviour
{
    public ColorButton gradientStop0ColorButton;
    public ColorButton gradientStop1ColorButton;
    public SyncedToggle gradientMappingToggle;
    public Material gradientPreviewMaterial;
    public Screen screen;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnChangeGradientMapping()
    {
        var isEnabled = gradientMappingToggle.isOn;

        if (isEnabled)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        else
        {
            canvasGroup.alpha = 0.25f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        screen.OnChangeGradientMapping();
    }

    public void OnChangeGradientStop0Color()
    {
        gradientPreviewMaterial.SetColor("_Stop0Color", gradientStop0ColorButton.color);
        screen.OnChangeGradientStop0Color();
    }

    public void OnChangeGradientStop1Color()
    {
        gradientPreviewMaterial.SetColor("_Stop1Color", gradientStop1ColorButton.color);
        screen.OnChangeGradientStop1Color();
    }
}
