
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Camera0Controller : UdonSharpBehaviour
{
    public ColorButton clearColorButton;
    public SyncedSlider fieldOfViewSlider;
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

    public void OnChangeFieldOfView()
    {
        cameraComponent.fieldOfView = fieldOfViewSlider.value;
    }
}
