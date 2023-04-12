
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PresetOption : UdonSharpBehaviour
{
    public string presetCode = "VkYAAT+AAAAAAAAAAQ==";
    public PresetBoard presetBoard;

    public void OnClick()
    {
        presetBoard.selectedOptionPresetCode = presetCode;
        presetBoard.OnClickPresetOptionButton();
    }
}
