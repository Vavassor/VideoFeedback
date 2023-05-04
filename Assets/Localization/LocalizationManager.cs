using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LocalizationManager : UdonSharpBehaviour
{
    /// <summary>
    /// Default locale as an IETF language tag.
    /// </summary>
    public string defaultLocale = "en";
    /// <summary>
    /// Current locale as an IETF language tag.
    /// </summary>
    public string locale = "en";
    /// <summary>
    /// Locales for each resource, as IETF language tags.
    /// </summary>
    public string[] locales;
    /// <summary>
    /// The placeholder text to return when a key is not found.
    /// </summary>
    public string placeholderValue = "Unknown";
    /// <summary>
    /// JSON resource files for each locale.
    /// </summary>
    public TextAsset[] resources;
    /// <summary>
    /// UI text that should be localized.
    /// </summary>
    public LocalizedText[] texts;

    private DataDictionary resourcesByLocale = new DataDictionary();

    public void SetLocale(string newLocale)
    {
        locale = newLocale;

        foreach (var text in texts)
        {
            var foundValue = FindValue(text.key, locale);
            if (foundValue == null)
            {
                foundValue = FindValue(text.key, defaultLocale);
            }
            if (foundValue == null)
            {
                foundValue = placeholderValue;
            }
            text.SetText(foundValue);
        }
    }

    void Start()
    {
        LoadResources();
        SetLocale(defaultLocale);
    }

    private void LoadResources()
    {
        for (var i = 0; i < locales.Length; i++)
        {
            var locale = locales[i];
            var isDeserialized = VRCJson.TryDeserializeFromJson(resources[i].text, out DataToken result);
            if (!isDeserialized)
            {
                Debug.LogError("Failed to load locale " + locale + ".");
            }
            else
            {
                resourcesByLocale.Add(locale, result);
            }
        }
    }

    private string FindValue(string key, string searchLocale)
    {
        string[] parts = key.Split('.');

        if (parts.Length != 2)
        {
            return null;
        }

        var segmentName = parts[0];
        var keyName = parts[1];

        var hasResource = resourcesByLocale.TryGetValue(searchLocale, out DataToken resourceToken);
        if (!hasResource || resourceToken.TokenType != TokenType.DataDictionary)
        {
            return null;
        }

        var resource = resourceToken.DataDictionary;
        var hasSegment = resource.TryGetValue(segmentName, out DataToken segmentToken);
        if (!hasSegment || segmentToken.TokenType != TokenType.DataDictionary)
        {
            return null;
        }

        var segment = segmentToken.DataDictionary;
        var hasValue = segment.TryGetValue(keyName, out DataToken valueToken);
        if (!hasValue || valueToken.TokenType != TokenType.String)
        {
            return null;
        }

        return valueToken.String;
    }
}
