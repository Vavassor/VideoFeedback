
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Camera0Controller : UdonSharpBehaviour
{
    public SyncedToggle orthographicProjectionToggle;

    private Camera cameraComponent;

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
    }

    public void OnChangeOrthographicProjection()
    {
        cameraComponent.orthographic = orthographicProjectionToggle.isOn;
    }
}
