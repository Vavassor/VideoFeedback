
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class PostProcessToggle : UdonSharpBehaviour
{
    public Toggle toggle;
    public PostProcessToggleSetting setting;

    public void OnChangeValue()
    {
        setting.OnChangeValue(toggle);
    }
}
