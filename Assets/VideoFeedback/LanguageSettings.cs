
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LanguageSettings : UdonSharpBehaviour
{
    public LocalizationManager localizationManager;

    public void OnClickEnglish()
    {
        localizationManager.SetLocale("en");
    }

    public void OnClickJapanese()
    {
        localizationManager.SetLocale("ja");
    }

    public void OnClickPseudolocale()
    {
        localizationManager.SetLocale("xa");
    }
}
