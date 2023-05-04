
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LocalizedText : UdonSharpBehaviour
{
    public string[] interpolationKeys;
    public string[] interpolationValues;
    public string key;
    /// <summary>
    /// Convert all characters to uppercase.
    /// Similar to the CSS property text-transform.
    /// </summary>
    public bool shouldTransformUppercase;

    private Text textComponent;
    private bool isInitialized;

    private void EnsureInitialized()
    {
        if (isInitialized)
        {
            return;
        }

        textComponent = GetComponent<Text>();

        isInitialized = true;
    }

    public void SetText(string text)
    {
        EnsureInitialized();
        textComponent.text = text;
    }
}
