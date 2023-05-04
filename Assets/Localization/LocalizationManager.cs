using System.Globalization;
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

            var interpolatedValue = Interpolate(text, foundValue);
            var transformedValue = Transform(text, interpolatedValue);
            text.SetText(transformedValue);
        }
    }

    private void Start()
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

    private string Interpolate(LocalizedText localizedText, string newText)
    {
        if (localizedText.interpolationKeys.Length == 0)
        {
            return newText;
        }

        var result = "";

        for (var i = 0; i < newText.Length; i++)
        {
            switch (newText[i])
            {
                case '{':
                {
                    if (i < newText.Length - 2 && newText[i + 1] == '{')
                    {
                        int codeStart = i + 2;
                        int codeEnd = newText.IndexOf("}}", codeStart);
                        if (codeEnd == -1)
                        {
                            Debug.LogError("Failed to get value. Invalid interpolation code.");
                            return newText;
                        }

                        var code = newText.Substring(codeStart, codeEnd - codeStart).Trim();
                        var key = code;

                        var interpolationKeyIndex = FindInterpolationKey(localizedText.interpolationKeys, key);
                        if (interpolationKeyIndex == -1)
                        {
                            Debug.Log("Failed to get value. Unknown interpolation key \"" + key + "\".");
                            return newText;
                        }

                        result += localizedText.interpolationValues[interpolationKeyIndex];
                        i = codeEnd + 1;
                    }
                    else
                    {
                        result += newText[i];
                    }
                    break;
                }
                default:
                {
                    result += newText[i];
                    break;
                }
            }
        }

        return result;
    }

    private int FindInterpolationKey(string[] interpolationKeys, string key)
    {
        for (var i = 0; i < interpolationKeys.Length; i++)
        {
            if (interpolationKeys[i].Equals(key))
            {
                return i;
            }
        }
        return -1;
    }

    private string Transform(LocalizedText localizedText, string newText)
    {
        if (localizedText.shouldTransformUppercase)
        {
            // Unfortunately, CultureInfo is not exposed to Udon. So we can't tell it to convert
            // to uppercase based on locale. Instead, use the invariant locale.
            return newText.ToUpperInvariant();
        }
        return newText;
    }
}
