using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;

[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
public class ResizeMenu : UdonSharpBehaviour
{
    [UdonSynced]
    public float menuLocalScale = 1.0f;

    public void OnRespawn()
    {
        SetMenuLocalScale(1.0f);
    }

    public void OnClickResizeHalf()
    {
        SetMenuLocalScale(0.5f);
    }

    public void OnClickResize1x()
    {
        SetMenuLocalScale(1.0f);
    }

    public void OnClickResizeQuarter()
    {
        SetMenuLocalScale(0.25f);
    }

    public override void OnDeserialization(DeserializationResult deserializationResult)
    {
        base.OnDeserialization(deserializationResult);
        transform.parent.localScale = new Vector3(menuLocalScale, menuLocalScale, menuLocalScale);
    }

    private void SetMenuLocalScale(float scale)
    {
        if (!Networking.IsOwner(gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        menuLocalScale = scale;

        transform.parent.localScale = new Vector3(scale, scale, scale);
    }
}
