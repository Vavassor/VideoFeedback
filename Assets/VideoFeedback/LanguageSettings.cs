
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using KPLocalization;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LanguageSettings : UdonSharpBehaviour
{
    public LocalizationManager localizationManager;

    public void OnClickEnglish()
    {
        localizationManager.SetPreferredLocale("en");
    }

    public void OnClickKorean()
    {
        localizationManager.SetPreferredLocale("ko");
    }

    public void OnClickJapanese()
    {
        localizationManager.SetPreferredLocale("ja");
    }

    public void OnClickPseudolocale()
    {
        localizationManager.SetPreferredLocale("xa");
    }
}
