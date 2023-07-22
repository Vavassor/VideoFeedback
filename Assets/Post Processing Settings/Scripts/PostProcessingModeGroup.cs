
using UdonSharp;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PostProcessingModeGroup : UdonSharpBehaviour
{
    public PostProcessVolume[] postProcessVolumes;
    public Toggle[] toggles;

    public void SetIsVolumeEnabled(bool isEnabled)
    {
        foreach (var toggle in toggles)
        {
            postProcessVolumes[0].enabled = isEnabled && toggle.isOn;
            postProcessVolumes[1].enabled = isEnabled && !toggle.isOn;
        }
    }

    public void OnChangeToggle0()
    {
        foreach (var toggle in toggles)
        {
            postProcessVolumes[0].enabled = toggle.isOn;
            postProcessVolumes[1].enabled = !toggle.isOn;
        }
    }
}
