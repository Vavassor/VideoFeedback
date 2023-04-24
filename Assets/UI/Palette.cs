
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Palette : UdonSharpBehaviour
{
    public Color[] colors;
    public ColorButton[] colorButtons;

    public void OnUpdatePalette()
    {
        foreach (var colorButton in colorButtons)
        {
            colorButton.OnUpdatePalette();
        }
    }
}
