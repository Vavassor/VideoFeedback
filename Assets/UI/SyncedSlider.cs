
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

    private bool isDeserializing;
    private Slider slider;
    private UdonBehaviour[] targetBehaviours;

    void Start()
    {
        slider = GetComponent<Slider>();
        targetBehaviours = (UdonBehaviour[])target.GetComponents(typeof(UdonBehaviour));
        isDeserializing = false;
    }

    public override void OnDeserialization()
    {
        isDeserializing = true;
        base.OnDeserialization();
        if (!Networking.IsOwner(gameObject))
        {
            ApplyValue();
        }
        isDeserializing = false;
    }

    public void OnChangeValue()
    {
        if (!Networking.IsOwner(gameObject) && !isDeserializing)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        value = slider.value;
        RequestSerialization();
        ApplyValue();
    }

    public void OnSetValueExternally()
    {
        if (!Networking.IsOwner(gameObject) && !isDeserializing)
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

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
