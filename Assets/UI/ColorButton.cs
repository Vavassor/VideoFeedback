
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
