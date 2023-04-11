// The read and write functions are from UNet by Xytabich. https://github.com/Xytabich/UNet
// For licensing see /licenses/LICENSE-unet.txt
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PresetBoard : UdonSharpBehaviour
{
    private const int BIT8 = 8;
    private const int BIT16 = 16;
    private const int BIT24 = 24;
    private const int BIT32 = 32;
    private const int BIT40 = 40;
    private const int BIT48 = 48;
    private const int BIT56 = 56;

    private const uint FLOAT_SIGN_BIT = 0x80000000;
    private const uint FLOAT_EXP_MASK = 0x7F800000;
    private const uint FLOAT_FRAC_MASK = 0x007FFFFF;

    public SyncedSlider brightnessSlider;
    public SyncedToggle clearCameraToggle;
    public SyncedSlider hueShiftSlider;
    public SyncedToggle mirrorXToggle;
    public SyncedToggle mirrorYToggle;
    public InputField presetCodeInputField;

    private float brightness;
    private bool clearCamera;
    private float hueShift;
    private bool mirrorX;
    private bool mirrorY;

    private const int codeSizeBytes = 11;
    private const string base64Digits = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    private const string defaultPresetCode = "P4AAAAAAAAABAAA=";

    public void OnClickGeneratePresetCode()
    {
        brightness = brightnessSlider.value;
        clearCamera = clearCameraToggle.isOn;
        hueShift = hueShiftSlider.value;
        mirrorX = mirrorXToggle.isOn;
        mirrorY = mirrorYToggle.isOn;

        presetCodeInputField.text = SerializePreset();
    }

    public void OnClickLoadPresetCode()
    {
        var didDeserializeSucceed = DeserializePreset(presetCodeInputField.text.Trim());
        if (didDeserializeSucceed)
        {
            ApplyPreset();
        }
    }

    public void OnClickResetAllButton()
    {
        DeserializePreset(defaultPresetCode);
        ApplyPreset();
    }

    public string SerializePreset()
    {
        byte[] bytes = new byte[codeSizeBytes];
        WriteSingle(brightness, bytes, 0);
        WriteSingle(hueShift, bytes, 4);
        WriteBool(clearCamera, bytes, 8);
        WriteBool(mirrorX, bytes, 9);
        WriteBool(mirrorY, bytes, 10);
        return ToBase64String(bytes);
    }

    public bool DeserializePreset(string text)
    {
        var bytes = FromBase64String(text);

        if (bytes.Length != codeSizeBytes)
        {
            return false;
        }

        brightness = ReadSingle(bytes, 0);
        hueShift = ReadSingle(bytes, 4);
        clearCamera = ReadBool(bytes, 8);
        mirrorX = ReadBool(bytes, 9);
        mirrorY = ReadBool(bytes, 10);

        return true;
    }

    private void ApplyPreset()
    {
        brightnessSlider.value = brightness;
        brightnessSlider.SendCustomEvent("OnSetValueExternally");

        clearCameraToggle.isOn = clearCamera;
        clearCameraToggle.SendCustomEvent("OnSetValueExternally");

        hueShiftSlider.value = hueShift;
        hueShiftSlider.SendCustomEvent("OnSetValueExternally");

        mirrorXToggle.isOn = mirrorX;
        mirrorXToggle.SendCustomEvent("OnSetValueExternally");

        mirrorYToggle.isOn = mirrorY;
        mirrorYToggle.SendCustomEvent("OnSetValueExternally");
    }

    private byte Base64ToSextet(char c)
    {
        return (byte) (c >= 'A' && c <= 'Z'
          ? c - 65
          : c >= 'a' && c <= 'z'
          ? c - 71
          : c >= '0' && c <= '9'
          ? c + 4
          : c == '+'
          ? 62
          : c == '/'
          ? 63
          : 0);
    }

    public byte[] FromBase64String(string text)
    {
        int byteCount = (text.Length * 3) / 4;
        int textLength = text.Length;

        // Trim padding characters.
        for (var i = text.Length - 1; i >= 0; i--)
        {
            if(text[i] != '=')
            {
                break;
            }

            textLength--;
            byteCount--;
        }

        byte[] bytes = new byte[byteCount];

        for (var i = 0; i < textLength; i += 4)
        {
            var c0 = Base64ToSextet(text[i]);
            var c1 = Base64ToSextet(text[i + 1]);

            var writeIndex = (i * 3) / 4;
            bytes[writeIndex] = (byte) ((c0 << 2) | ((c1 & 0x30) >> 4));

            // If the last two characters aren't padding.
            if (i + 2 < textLength)
            {
                var c2 = Base64ToSextet(text[i + 2]);
                bytes[writeIndex + 1] = (byte)(((c1 & 0xf) << 4) | ((c2 & 0x3c) >> 2));

                // If the last character isn't padding.
                if (i + 3 < textLength)
                {
                    var c3 = Base64ToSextet(text[i + 3]);
                    bytes[writeIndex + 2] = (byte)(((c2 & 0x3) << 6) | c3);
                }
            }
        }

        return bytes;
    }

    public string ToBase64String(byte[] bytes)
    {
        string result = string.Empty;
        var caseIndex = 0;

        for (var i = 0; i < bytes.Length; caseIndex++)
        {
            var currentByte = bytes[i];
            var nextByte = i < bytes.Length - 1 ? bytes[i + 1] : (byte) 0;

            switch (caseIndex % 4)
            {
                case 0:
                {
                    result += base64Digits[(currentByte & 0xfc) >> 2];
                    break;
                }
                case 1:
                {
                    result += base64Digits[((currentByte & 0x3) << 4) | ((nextByte & 0xf0) >> 4)];
                    i++;
                    break;
                }
                case 2:
                {
                    result += base64Digits[((currentByte & 0xf) << 2) | ((nextByte & 0xc0) >> 6)];
                    i++;
                    break;
                }
                case 3:
                {
                    result += base64Digits[currentByte & 0x3f];
                    i++;
                    break;
                }
            }
        }

        // Pad to a multiple of 4.
        for (var i = 0; i < result.Length % 4; i++)
        {
            result += "=";
        }

        return result;
    }

    public bool ReadBool(byte[] buffer, int index)
    {
        return buffer[index] == 1;
    }

    public uint ReadUInt32(byte[] buffer, int index)
    {
        uint value = 0;
        value |= (uint)buffer[index] << BIT24;
        index++;
        value |= (uint)buffer[index] << BIT16;
        index++;
        value |= (uint)buffer[index] << BIT8;
        index++;
        value |= (uint)buffer[index];
        return value;
    }

    public float ReadSingle(byte[] buffer, int index)
    {
        uint value = ReadUInt32(buffer, index);
        if (value == 0 || value == FLOAT_SIGN_BIT) return 0f;

        int exp = (int)((value & FLOAT_EXP_MASK) >> 23);
        int frac = (int)(value & FLOAT_FRAC_MASK);
        bool negate = (value & FLOAT_SIGN_BIT) == FLOAT_SIGN_BIT;
        if (exp == 0xFF)
        {
            if (frac == 0)
            {
                return negate ? float.NegativeInfinity : float.PositiveInfinity;
            }
            return float.NaN;
        }

        bool normal = exp != 0x00;
        if (normal) exp -= 127;
        else exp = -126;

        float result = frac / (float)(2 << 22);
        if (normal) result += 1f;

        result *= Mathf.Pow(2, exp);
        if (negate) result = -result;
        return result;
    }

    public int WriteBool(bool value, byte[] buffer, int index)
    {
        if (value) buffer[index] = 1;
        else buffer[index] = 0;
        return 1;
    }

    public int WriteUInt32(uint value, byte[] buffer, int index)
    {
        buffer[index] = (byte)((value >> BIT24) & 255u);
        index++;
        buffer[index] = (byte)((value >> BIT16) & 255u);
        index++;
        buffer[index] = (byte)((value >> BIT8) & 255u);
        index++;
        buffer[index] = (byte)(value & 255u);
        return 4;
    }

    public int WriteSingle(float value, byte[] buffer, int index)
    {
        uint tmp = 0;
        if (float.IsNaN(value))
        {
            tmp = FLOAT_EXP_MASK | FLOAT_FRAC_MASK;
        }
        else if (float.IsInfinity(value))
        {
            tmp = FLOAT_EXP_MASK;
            if (float.IsNegativeInfinity(value)) tmp |= FLOAT_SIGN_BIT;
        }
        else if (value != 0f)
        {
            if (value < 0f)
            {
                value = -value;
                tmp |= FLOAT_SIGN_BIT;
            }

            int exp = 0;
            bool normal = true;
            while (value >= 2f)
            {
                value *= 0.5f;
                exp++;
            }
            while (value < 1f)
            {
                if (exp == -126)
                {
                    normal = false;
                    break;
                }
                value *= 2f;
                exp--;
            }

            if (normal)
            {
                value -= 1f;
                exp += 127;
            }
            else exp = 0;

            tmp |= Convert.ToUInt32(exp << 23) & FLOAT_EXP_MASK;
            tmp |= Convert.ToUInt32(value * (2 << 22)) & FLOAT_FRAC_MASK;
        }
        return WriteUInt32(tmp, buffer, index);
    }
}
