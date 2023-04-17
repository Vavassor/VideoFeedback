
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedToggle : UdonSharpBehaviour
{
    public GameObject[] enableGameObjects;
    [UdonSynced]
    public bool isOn;
    public GameObject target;
    public string changeToggleEventName = "OnChangeToggle";

    private Toggle toggle;
    private UdonBehaviour[] targetBehaviours;

    public override void OnDeserialization()
    {
        base.OnDeserialization();
        ApplyToggle();
    }

    public void OnChangeValue()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        isOn = toggle.isOn;
        RequestSerialization();
        ApplyToggle();
    }

    public void OnSetValueExternally()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
        RequestSerialization();
        ApplyToggle();
    }

    private void ApplyToggle()
    {
        toggle.isOn = isOn;

        foreach (var targetBehaviour in targetBehaviours)
        {
            targetBehaviour.SendCustomEvent(changeToggleEventName);
        }

        foreach (var obj in enableGameObjects)
        {
            obj.SetActive(isOn);
        }
    }

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        targetBehaviours = (UdonBehaviour[]) target.GetComponents(typeof(UdonBehaviour));
    }
}
