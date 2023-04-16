
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public SyncedToggle cameraClearToggle;
    public CustomRenderTexture videoMixed;
    public SyncedToggle orthographicProjectionToggle;

    private Camera cameraComponent;

    public void OnChangeCameraClear()
    {
        cameraComponent.clearFlags = cameraClearToggle.isOn ? CameraClearFlags.SolidColor : CameraClearFlags.Depth;
    }

    public void OnChangeOrthographicProjection()
    {
        cameraComponent.orthographic = orthographicProjectionToggle.isOn;
    }

    void Start()
    {
        cameraComponent = GetComponent<Camera>();

        videoMixed.Initialize();
    }

    void Update()
    {
        videoMixed.Update();
    }
}
