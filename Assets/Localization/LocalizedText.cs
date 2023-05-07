
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LocalizedText : UdonSharpBehaviour
{
    /// <summary>
    /// A context for context-specific variants.
    /// <example>
    /// For example, the context could be gender for gender-specific translations.
    /// </example>
    /// </summary>
    public string context;
    /// <summary>
    /// How many items are relevant for plural forms.
    /// </summary>
    public int count;
    /// <summary>
    /// Keys for interpolation.
    /// <example>
    /// For example, "count" would be the key in the following translation.
    /// <code>
    /// {{count}} items
    /// </code>
    /// </example>
    /// </summary>
    public string[] interpolationKeys;
    /// <summary>
    /// Values for interpolation.
    /// <example>
    /// For example, suppose we had the following translation.
    /// <code>
    /// {{count}} items
    /// </code>
    /// A value of 100 would be interpolated into the text "100 items".
    /// </example>
    /// </summary>
    public string[] interpolationValues;
    public string key;
    /// <summary>
    /// Convert all characters to uppercase.
    /// Similar to the CSS property text-transform.
    /// </summary>
    public bool shouldTransformUppercase;
    public bool shouldUseCount;

    private bool isInitialized;
    private Text textComponent;

    private void EnsureInitialized()
    {
        if (!isInitialized)
        {
            textComponent = GetComponent<Text>();

            isInitialized = true;
        }
    }

    public void SetText(string text)
    {
        EnsureInitialized();
        textComponent.text = text;
    }
}
