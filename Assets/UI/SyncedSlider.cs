
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedSlider : UdonSharpBehaviour
{
    public string changeSliderEventName = "OnChangeSlider";
    public GameObject target;
    [UdonSynced]
    public float value;

    private bool isInitialized;
    private Slider slider;
    private UdonBehaviour[] targetBehaviours;

    void Start()
    {
        Initialize();
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
        Initialize();
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

    private void Initialize()
    {
        if (isInitialized)
        {
            return;
        }

        slider = GetComponent<Slider>();
        targetBehaviours = (UdonBehaviour[])target.GetComponents(typeof(UdonBehaviour));

        isInitialized = true;
    }
}
