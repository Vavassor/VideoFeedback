
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

/// <summary>
/// Localization information for a UI text component.
/// 
/// <see cref="LocalizationManager" /> is responsible for most functionality.
/// </summary>
/// <remarks>
/// Prefer using synchronization method "None". This isn't enforced with UdonBehaviourSyncMode
/// because behaviours with other sync methods may need to be placed on the same GameObject.
/// </remarks>
[RequireComponent(typeof(Text))]
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
    /// Convert all text to uppercase.
    /// 
    /// Similar to the CSS property text-transform: uppercase.
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
