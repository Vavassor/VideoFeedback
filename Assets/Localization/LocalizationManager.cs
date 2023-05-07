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

    public void SetLocale(string newLocale)
    {
        locale = newLocale;

        foreach (var text in texts)
        {
            var foundValue = FindValue(text, locale);

            if (foundValue == null)
            {
                foundValue = FindValue(text, defaultLocale);
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

    private string FindValue(LocalizedText localizedText, string searchLocale)
    {
        var key = localizedText.key;
        string[] parts = key.Split('.');

        if (parts.Length != 2)
        {
            return null;
        }

        var segmentName = parts[0];
        var keyName = parts[1];

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

        // If the key could be one of several variants, use the key name for the specific variant.
        var variantKeyName = keyName;

        if (localizedText.context != null && localizedText.context.Length > 0)
        {
            variantKeyName += "_" + localizedText.context;
        }

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
    /// <see href="http://translate.sourceforge.net/wiki/l10n/pluralforms">Plural Forms Localization Guide</see>
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
