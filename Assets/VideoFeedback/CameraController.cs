
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CameraController : UdonSharpBehaviour
{
    public SyncedToggle cameraClearToggle;
    public RenderTexture video0;
    public RenderTexture video1;

    private Camera cameraComponent;

    public void OnChangeCameraClear()
    {
        cameraComponent.clearFlags = cameraClearToggle.isOn ? CameraClearFlags.SolidColor : CameraClearFlags.Nothing;
    }

    void Start()
    {
        cameraComponent = GetComponent<Camera>();
    }

    void Update()
    {
        VRCGraphics.Blit(video0, video1);
    }
}
