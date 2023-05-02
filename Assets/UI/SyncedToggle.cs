
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SyncedToggle : UdonSharpBehaviour
{
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
        EnsureOwnership();
        isOn = toggle.isOn;
        RequestSerialization();
        ApplyToggle();
    }

    public void OnSetValueExternally()
    {
        EnsureOwnership();
        RequestSerialization();
        ApplyToggle();
    }

    public void Randomize()
    {
        isOn = Random.value > 0.5f;
        OnSetValueExternally();
    }

    private void ApplyToggle()
    {
        toggle.isOn = isOn;

        foreach (var targetBehaviour in targetBehaviours)
        {
            targetBehaviour.SendCustomEvent(changeToggleEventName);
        }
    }

    private void Start()
    {
        toggle = GetComponent<Toggle>();
        targetBehaviours = (UdonBehaviour[]) target.GetComponents(typeof(UdonBehaviour));
    }

    private void EnsureOwnership()
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
    }
}
