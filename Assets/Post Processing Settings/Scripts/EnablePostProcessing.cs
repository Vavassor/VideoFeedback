
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

public class EnablePostProcessing : UdonSharpBehaviour
{
    public CanvasGroup postProcessCanvasGroup;
    public Toggle postProcessingToggle;
    public PostProcessSetting[] settings;
    public PostProcessingModeGroup[] modeGroups;
    public PostProcessToggleSetting[] toggleSettings;

    public void OnChangePostProcessing()
    {
        var isEnabled = postProcessingToggle.isOn;

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

        foreach (var setting in settings)
        {
            setting.SetIsVolumeEnabled(isEnabled);
        }

        foreach (var modeGroup in modeGroups)
        {
            modeGroup.SetIsVolumeEnabled(isEnabled);
        }

        foreach (var toggleSetting in toggleSettings)
        {
            toggleSetting.SetIsVolumeEnabled(isEnabled);
        }
    }
}
