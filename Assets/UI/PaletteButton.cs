
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PaletteButton : UdonSharpBehaviour
{
    public Color color;
    public ColorButton colorButton;
    public int paletteColorIndex;

    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void OnClickButton()
    {
        colorButton.color = image.color;
        colorButton.OnSetValueExternally();
    }

    public void OnUpdatePalette()
    {
        var palette = colorButton.palette;

        if (palette != null)
        {
            image.color = palette.colors[paletteColorIndex];
        }
    }
}
