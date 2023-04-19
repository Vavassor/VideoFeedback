﻿
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
    public CanvasGroup modal;
    public SyncedSlider saturationSlider;
    public SyncedSlider valueSlider;
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

    public void OnDisableButton()
    {
        button.interactable = false;
    }

    public void OnDismiss()
    {
        modal.alpha = 0.0f;
        modal.blocksRaycasts = false;
        modal.interactable = false;
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
