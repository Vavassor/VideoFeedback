
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class PostProcessSlider : UdonSharpBehaviour
{
    public PostProcessSetting setting;
    public Slider slider;

    public void OnChangeValue()
    {
        setting.OnChangeValue(slider);
    }
}
