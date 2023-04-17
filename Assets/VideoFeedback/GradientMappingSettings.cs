
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GradientMappingSettings : UdonSharpBehaviour
{
    public ColorButton gradientStop0ColorButton;
    public ColorButton gradientStop1ColorButton;
    public SyncedToggle gradientMappingToggle;
    public Screen screen;

    public void OnChangeGradientMapping()
    {
        var isEnabled = gradientMappingToggle.isOn;

        if (isEnabled)
        {
            gradientStop0ColorButton.OnEnableButton();
            gradientStop1ColorButton.OnEnableButton();
        }
        else
        {
            gradientStop0ColorButton.OnDisableButton();
            gradientStop1ColorButton.OnDisableButton();
        }

        screen.OnChangeGradientMapping();
    }
}
