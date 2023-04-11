
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Screen : UdonSharpBehaviour
{
    public Material material;
    public SyncedSlider brightnessSlider;
    public SyncedSlider hueShiftSlider;
    public SyncedToggle mirrorXToggle;
    public SyncedToggle mirrorYToggle;
    public float screenScaleX = 4.0f;
    public float screenScaleY = 2.25f;

    private float scaleX;
    private float scaleY;

    void Start()
    {
        scaleX = screenScaleX;
        scaleY = screenScaleY;
    }

    public void OnChangeBrightness()
    {
        material.SetFloat("_Brightness", brightnessSlider.value);
    }

    public void OnChangeHueShift()
    {
        material.SetFloat("_HueShift", hueShiftSlider.value);
    }

    public void OnChangeMirrorX()
    {
        scaleX = mirrorXToggle.isOn ? -screenScaleX : screenScaleX;
        UpdateScale();
    }

    public void OnChangeMirrorY()
    {
        scaleY = mirrorYToggle.isOn ? -screenScaleY : screenScaleY;
        UpdateScale();
    }

    private void UpdateScale()
    {
        transform.localScale = new Vector3(scaleX, scaleY, 1.0f);
    }
}
