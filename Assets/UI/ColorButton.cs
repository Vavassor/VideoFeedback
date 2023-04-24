
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ColorButton : UdonSharpBehaviour
{
    public string changeEventName = "OnChangeColor";
    public Color color;
    public int paletteColorIndex;
    public SyncedSlider hueSlider;
    public CanvasGroup modal;
    public Palette palette;
    public PaletteButton[] paletteButtons;
    public SyncedSlider saturationSlider;
    public Material saturationSliderMaterial;
    public SyncedSlider valueSlider;
    public Material valueSliderMaterial;
    // The object to notify for events.
    public GameObject target;
    // Show the modal within this UI element. Usually a Canvas, CanvasGroup, or Panel.
    public GameObject modalContainer;

    private Button button;
    private UdonBehaviour[] targetBehaviours;

    void Start()
    {
        button = GetComponent<Button>();
        targetBehaviours = (UdonBehaviour[]) target.GetComponents(typeof(UdonBehaviour));
        
        if (modalContainer != null)
        {
            // To ensure the modal shows on top of everything else, place it last after other UI.
            // elements. Setting its parent makes it the last element within that group of elements.
            modal.transform.SetParent(modalContainer.transform);
        }
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

    public void OnClickButton()
    {
        modal.alpha = 1.0f;
        modal.blocksRaycasts = true;
        modal.interactable = true;
    }

    public void OnDismiss()
    {
        modal.alpha = 0.0f;
        modal.blocksRaycasts = false;
        modal.interactable = false;
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

        OnUpdatePalette();
    }

    public void OnGetColor()
    {
        var pickedColor = Color.HSVToRGB(hueSlider.value, saturationSlider.value, valueSlider.value);
        color = pickedColor;
    }

    public void OnUpdatePalette()
    {
        foreach (var paletteButton in paletteButtons)
        {
            paletteButton.OnUpdatePalette();
        }
    }

    private void UpdateColor()
    {
        var hue = hueSlider.value;
        var saturation = saturationSlider.value;
        var value = valueSlider.value;

        var pickedColor = Color.HSVToRGB(hue, saturation, value);
        button.image.color = pickedColor;
        color = pickedColor;

        saturationSliderMaterial.SetColor("_Stop0Color", Color.HSVToRGB(hue, 0.0f, value));
        saturationSliderMaterial.SetColor("_Stop1Color", Color.HSVToRGB(hue, 1.0f, value));
        valueSliderMaterial.SetColor("_Stop1Color", Color.HSVToRGB(hue, saturation, 1.0f));

        palette.colors[paletteColorIndex] = pickedColor;
        palette.OnUpdatePalette();

        foreach (var targetBehaviour in targetBehaviours)
        {
            targetBehaviour.SendCustomEvent(changeEventName);
        }
    }
}
