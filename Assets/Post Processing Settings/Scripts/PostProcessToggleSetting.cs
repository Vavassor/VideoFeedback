
using UdonSharp;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PostProcessToggleSetting : UdonSharpBehaviour
{
    public PostProcessVolume postProcessVolume;
    public Toggle[] toggles;
    public bool isOn;

    public void SetIsVolumeEnabled(bool isEnabled)
    {
        postProcessVolume.enabled = isEnabled;
    }

    public void OnChangeValue(Toggle toggle)
    {
        if (toggle.isOn == isOn)
        {
            return;
        }

        isOn = toggle.isOn;
        postProcessVolume.weight = isOn ? 1.0f : 0.0f;

        foreach (var otherToggle in toggles)
        {
            if (otherToggle != toggle)
            {
                otherToggle.isOn = isOn;
            }
        }
    }
}
