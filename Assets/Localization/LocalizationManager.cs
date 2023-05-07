using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// Localization settings and functionality for a world.
/// </summary>
/// <remarks>
/// It's recommended to use one manager per world. But multiple managers can be used as long as
/// they don't share target assets.
/// </remarks>
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LocalizationManager : UdonSharpBehaviour
{
    /// <summary>
    /// The default locale as an IETF language tag. This should be the language of your source translation.
    /// </summary>
    public string defaultLocale = "en";
    /// <summary>
    /// The user's preferred languages in preference order, as IETF language tags.
    /// </summary>
    public string[] preferredLocales = { "en" };
    /// <summary>
    /// The placeholder text to return when a key is not found.
    /// </summary>
    public string placeholderValue = "Unknown";
    /// <summary>
    /// The locales of each resource, as IETF language tags.
    /// </summary>
    public string[] resourceLocales;
    /// <summary>
    /// JSON resource files for each locale.
    /// 
    /// These are expected to have the following form.
    /// <code>
    /// {
    ///   "segmentName": {
    ///     "keyName": "value"
    ///   }
    /// }
    /// </code>
    /// </summary>
    public TextAsset[] resources;
    /// <summary>
    /// UI text that should be localized.
    /// </summary>
    public LocalizedText[] texts;

    private DataDictionary resourcesByLocale = new DataDictionary();

    /// <summary>
    /// Set the user's preferred locale.
    /// </summary>
    /// <param name="preferredLocale">The locale as an IETF language tag.</param>
    public void SetPreferredLocale(string preferredLocale)
    {
        SetPreferredLocales(new string[] { preferredLocale });
    }

    /// <summary>
    /// Set the user's preferred locales.
    /// </summary>
    /// <remarks>
    /// Use <see cref="SetPreferredLocale"/> if you don't need to support multi-lingual user preferences.
    /// </remarks>
    /// <param name="newPreferredLocales">The locales in order of preference, as IETF language tags.</param>
    public void SetPreferredLocales(string[] newPreferredLocales)
    {
        preferredLocales = newPreferredLocales;

        var searchLocales = GetSearchLocales();

        foreach (var text in texts)
        {
            var foundValue = FindValue(text, searchLocales);
            var interpolatedValue = Interpolate(text, foundValue);
            var transformedValue = Transform(text, interpolatedValue);
            text.SetText(transformedValue);
        }
    }

    private void Start()
    {
        LoadResources();
        SetPreferredLocale(defaultLocale);
    }

    private void LoadResources()
    {
        for (var i = 0; i < resourceLocales.Length; i++)
        {
            var locale = resourceLocales[i];
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

    /// <summary>
    /// Get a list of locales to search in order of preference.
    /// </summary>
    private string[] GetSearchLocales()
    {
        // Put all the locales in a DataList.
        DataList searchList = new DataList();
        var isDefaultInPreferences = false;

        for (var i = 0; i < preferredLocales.Length; i++)
        {
            var preferredLocale = preferredLocales[i];
            if (preferredLocale.Equals(defaultLocale))
            {
                isDefaultInPreferences = true;
            }

            // TODO: Add language variant fallbacks if the preferred language is regional.
            // For example, if a preferred locale was "es-MX" (Mexican Spanish), then we may
            // also want to search "es" (generic Spanish) before other locales.

            // TODO: Filter out locales which don't have resources. Ideally nobody would call
            // SetPreferredLanguages with languages they don't support. But we could log an error
            // message if they did.
            searchList.Add(preferredLocale);
        }

        if (!isDefaultInPreferences)
        {
            searchList.Add(defaultLocale);
        }

        // Convert the list to an array.
        var tokenArray = searchList.ToArray();
        var searchLocales = new string[tokenArray.Length];
        for(var i = 0; i < tokenArray.Length; i++)
        {
            searchLocales[i] = tokenArray[i].String;
        }

        return searchLocales;
    }

    private string FindValue(LocalizedText localizedText, string[] searchLocales)
    {
        // Parse the key.
        // A key has the form "segmentName.keyName_context_pluralCategory". The context and plural category are optional.
        var key = localizedText.key;
        string[] parts = key.Split('.');

        if (parts.Length != 2)
        {
            return null;
        }

        var segmentName = parts[0];
        var keyName = parts[1];

        // If the value is context-dependent, use the key for the specific context.
        var contextDependentKeyName = keyName;
        if (localizedText.context != null && localizedText.context.Length > 0)
        {
            contextDependentKeyName += "_" + localizedText.context;
        }

        // Search each locale for the value.
        foreach (var locale in searchLocales)
        {
            var foundValue = FindValueInLocale(localizedText, segmentName, contextDependentKeyName, locale);
            if (foundValue != null)
            {
                return foundValue;
            }
        }

        return placeholderValue;
    }

    private string FindValueInLocale(LocalizedText localizedText, string segmentName, string contextDependentKeyName, string searchLocale)
    {
        // Get the resource.
        var hasResource = resourcesByLocale.TryGetValue(searchLocale, out DataToken resourceToken);
        if (!hasResource || resourceToken.TokenType != TokenType.DataDictionary)
        {
            return null;
        }

        // Get the segment.
        var resource = resourceToken.DataDictionary;
        var hasSegment = resource.TryGetValue(segmentName, out DataToken segmentToken);
        if (!hasSegment || segmentToken.TokenType != TokenType.DataDictionary)
        {
            return null;
        }

        // If the value could be pluralized, use the key for that plural category.
        var variantKeyName = contextDependentKeyName;
        if (localizedText.shouldUseCount)
        {
            variantKeyName += "_" + GetCardinalPluralCategory(localizedText.count, searchLocale);
        }

        // Get the value.
        var segment = segmentToken.DataDictionary;
        var hasValue = segment.TryGetValue(variantKeyName, out DataToken valueToken);
        if (!hasValue || valueToken.TokenType != TokenType.String)
        {
            return null;
        }

        return valueToken.String;
    }

    /// <summary>
    /// Get the cardinal plural category.
    /// See <see href="http://translate.sourceforge.net/wiki/l10n/pluralforms">Plural Forms Localization Guide</see>
    /// </summary>
    /// <param name="n">Number of items for the plural word.</param>
    /// <param name="locale">IETF language tag</param>
    /// <returns>a plural category</returns>
    private string GetCardinalPluralCategory(int n, string locale)
    {
        // European portuguese is the only plural rule that's different compared to its primary language.
        if (locale.Equals("pt-PT"))
        {
            return n != 1 ? "other" : "one";
        }

        var languageSubtag = locale.Split('-')[0];

        switch (languageSubtag)
        {
            default:
                return "other";
            case "ach":
            case "ak":
            case "am":
            case "arn":
            case "br":
            case "fil":
            case "fr":
            case "gun":
            case "ln":
            case "mfe":
            case "mg":
            case "mi":
            case "oc":
            case "pt":
            case "tg":
            case "tl":
            case "ti":
            case "tr":
            case "uz":
            case "wa":
                return n > 1 ? "other" : "one";
            case "af":
            case "an":
            case "ast":
            case "az":
            case "bg":
            case "bn":
            case "ca":
            case "da":
            case "de":
            case "dev":
            case "el":
            case "en":
            case "eo":
            case "es":
            case "et":
            case "eu":
            case "fi":
            case "fo":
            case "fur":
            case "fy":
            case "gl":
            case "gu":
            case "ha":
            case "hi":
            case "hu":
            case "hy":
            case "ia":
            case "it":
            case "kk":
            case "kn":
            case "ku":
            case "lb":
            case "mai":
            case "ml":
            case "mn":
            case "mr":
            case "nah":
            case "nap":
            case "nb":
            case "ne":
            case "nl":
            case "nn":
            case "no":
            case "nso":
            case "or":
            case "pa":
            case "pap":
            case "pms":
            case "ps":
            case "rm":
            case "sco":
            case "se":
            case "si":
            case "so":
            case "son":
            case "sq":
            case "sv":
            case "sw":
            case "ta":
            case "te":
            case "tk":
            case "ur":
            case "yo":
                return n != 1 ? "other" : "one";
            case "ay":
            case "bo":
            case "cgg":
            case "fa":
            case "ht":
            case "id":
            case "ja":
            case "jbo":
            case "ka":
            case "km":
            case "ko":
            case "ky":
            case "lo":
            case "ms":
            case "sah":
            case "su":
            case "th":
            case "tt":
            case "ug":
            case "vi":
            case "wo":
            case "zh":
                return "other";
            case "be":
            case "bs":
            case "cnr":
            case "dz":
            case "hr":
            case "ru":
            case "sr":
            case "uk":
                return n % 10 == 1 && n % 100 != 11 ? "one" : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? "few" : "other";
            case "ar":
                return n == 0 ? "zero" : n == 1 ? "one" : n == 2 ? "two" : n % 100 >= 3 && n % 100 <= 10 ? "few" : n % 100 >= 11 ? "many" : "other";
            case "cs":
            case "sk":
                return (n == 1) ? "one" : (n >= 2 && n <= 4) ? "few" : "other";
            case "csb":
            case "pl":
                return n == 1 ? "one" : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? "few" : "many";
            case "cy":
                return (n == 1) ? "one" : (n == 2) ? "two" : (n != 8 && n != 11) ? "other" : "few";
            case "ga":
                return n == 1 ? "one" : n == 2 ? "two" : n < 7 ? "few" : n < 11 ? "many" : "other";
            case "gd":
                return (n == 1 || n == 11) ? "one" : (n == 2 || n == 12) ? "two" : (n > 2 && n < 20) ? "few" : "other";
            case "is":
                return n % 10 != 1 || n % 100 == 11 ? "other" : "one";
            case "jv":
                return n != 0 ? "other" : "zero";
            case "kw":
                return (n == 1) ? "one" : (n == 2) ? "two" : (n == 3) ? "few" : "other";
            case "lt":
                return n % 10 == 1 && n % 100 != 11 ? "one" : n % 10 >= 2 && (n % 100 < 10 || n % 100 >= 20) ? "few" : "other";
            case "lv":
                return n % 10 == 1 && n % 100 != 11 ? "one" : n != 0 ? "other" : "zero";
            case "mk":
                return n == 1 || n % 10 == 1 && n % 100 != 11 ? "one" : "other";
            case "mnk":
                return n == 0 ? "zero" : n == 1 ? "one" : "other";
            case "mt":
                return n == 1 ? "one" : n == 2 ? "two" : n == 0 || (n % 100 > 1 && n % 100 < 11) ? "few" : (n % 100 > 10 && n % 100 < 20) ? "many" : "other";
            case "ro":
                return n == 1 ? "one" : (n == 0 || (n % 100 > 0 && n % 100 < 20)) ? "few" : "other";
            case "sl":
                return n % 100 == 1 ? "one" : n % 100 == 2 ? "two" : n % 100 == 3 || n % 100 == 4 ? "few" : "other";
            case "he":
            case "iw":
                return n == 1 ? "one" : n == 2 ? "two" : (n < 0 || n > 10) && n % 10 == 0 ? "few" : "other";
        }
    }

    /// <summary>
    /// Text may have names or other text inserted into it. This is called interpolation.
    /// <example>
    /// For example "{{ playerName }} was added to team {{ teamName }}."
    /// </example>
    /// </summary>
    private string Interpolate(LocalizedText localizedText, string newText)
    {
        if (localizedText.interpolationKeys.Length == 0 && !localizedText.shouldUseCount)
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

                        string interpolationValue;
                        if (key.Equals("count"))
                        {
                            interpolationValue = localizedText.count.ToString();
                        }
                        else
                        {
                            var interpolationKeyIndex = FindInterpolationKey(localizedText.interpolationKeys, key);
                            if (interpolationKeyIndex == -1)
                            {
                                Debug.LogError("Failed to get value. Unknown interpolation key \"" + key + "\".");
                                return newText;
                            }

                            interpolationValue = localizedText.interpolationValues[interpolationKeyIndex];
                        }

                        result += interpolationValue;
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
