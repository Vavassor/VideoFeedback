
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedSlider : UdonSharpBehaviour
{
    public string changeSliderEventName = "OnChangeSlider";
    [UdonSynced]
    public float value;

    private Slider slider;
    public GameObject target;
    private UdonBehaviour[] targetBehaviours;

    void Start()
    {
        slider = GetComponent<Slider>();
        targetBehaviours = (UdonBehaviour[]) target.GetComponents(typeof(UdonBehaviour));
    }

    public override void OnDeserialization()
    {
        base.OnDeserialization();
        ApplyValue();
    }

    public void OnChangeValue()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        value = slider.value;
        RequestSerialization();
        ApplyValue();
    }

    public void OnSetValueExternally()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        RequestSerialization();
        ApplyValue();
    }

    private void ApplyValue()
    {
        slider.value = value;

        foreach (var targetBehaviour in targetBehaviours)
        {
            targetBehaviour.SendCustomEvent(changeSliderEventName);
        }
    }
}
