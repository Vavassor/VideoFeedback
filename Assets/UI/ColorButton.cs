
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ColorButton : UdonSharpBehaviour
{
    public string changeEventName = "OnChangeColor";
    public Color color;
    public SyncedSlider hueSlider;
    public SyncedSlider saturationSlider;
    public SyncedSlider valueSlider;
    public GameObject target;
    
    private Button button;
    private UdonBehaviour[] targetBehaviours;

    void Start()
    {
        button = GetComponent<Button>();
        targetBehaviours = (UdonBehaviour[]) target.GetComponents(typeof(UdonBehaviour));
    }

    public void OnChangeHue()
    {
        UpdateColor();
    }

    public void OnChangeSaturation()
    {
        UpdateColor();
    }

    public void OnChangeValue()
    {
        UpdateColor();
    }

    public void OnDisableButton()
    {
        button.interactable = false;
    }

    public void OnEnableButton()
    {
        button.interactable = true;
    }

    public void OnSetValueExternally()
    {
        Color.RGBToHSV(color, out float hue, out float saturation, out float value);

        hueSlider.value = hue;
        hueSlider.OnSetValueExternally();

        saturationSlider.value = saturation;
        saturationSlider.OnSetValueExternally();

        valueSlider.value = value;
        valueSlider.OnSetValueExternally();
    }

    public void OnGetColor()
    {
        var pickedColor = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
        color = pickedColor;
    }

    private void UpdateColor()
    {
        var pickedColor = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
        button.image.color = pickedColor;
        color = pickedColor;

        foreach (var targetBehaviour in targetBehaviours)
        {
            targetBehaviour.SendCustomEvent(changeEventName);
        }
    }
}
