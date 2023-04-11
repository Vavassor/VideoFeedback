
using System;
using UdonSharp;
using UnityEngine;
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

    private float brightness;
    private bool clearCamera;
    private float hueShift;
    private bool mirrorX;
    private bool mirrorY;

    private const string hexDigits = "0123456789ABCDEF";

    public void OnClickResetAllButton()
    {
        brightnessSlider.value = 1.0f;
        brightnessSlider.SendCustomEvent("OnSetValueExternally");

        clearCameraToggle.isOn = true;
        clearCameraToggle.SendCustomEvent("OnSetValueExternally");

        hueShiftSlider.value = 0.0f;
        hueShiftSlider.SendCustomEvent("OnSetValueExternally");

        mirrorXToggle.isOn = false;
        mirrorXToggle.SendCustomEvent("OnSetValueExternally");

        mirrorYToggle.isOn = false;
        mirrorYToggle.SendCustomEvent("OnSetValueExternally");
    }

    public string Serialize()
    {
        byte[] bytes = new byte[11];
        WriteSingle(brightness, bytes, 0);
        WriteSingle(hueShift, bytes, 4);
        WriteBool(clearCamera, bytes, 8);
        WriteBool(mirrorX, bytes, 9);
        WriteBool(mirrorY, bytes, 10);
        return ToBase16String(bytes);
    }

    public void Deserialize(string text)
    {
        var bytes = FromBase16String(text);
        brightness = ReadSingle(bytes, 0);
        hueShift = ReadSingle(bytes, 4);
        clearCamera = ReadBool(bytes, 8);
        mirrorX = ReadBool(bytes, 9);
        mirrorY = ReadBool(bytes, 10);
    }

    // This will give junk bytes when given a non-hexadecimal string.
    public byte[] FromBase16String(string text)
    {
        byte[] result = new byte[text.Length / 2];

        for (var i = 0; i < result.Length; i++)
        {
            var c0 = text[2 * i];
            var c1 = text[2 * i + 1];
            var b0 = (byte) ((c0 >= '0' && c0 <= '9') ? c0 - '0' : c0 - 'A' + 10);
            var b1 = (byte) ((c1 >= '0' && c1 <= '9') ? c1 - '0' : c1 - 'A' + 10);
            result[i] = (byte) ((b0 << 4) | b1);
        }

        return result;
    }

    public string ToBase16String(byte[] bytes)
    {
        string result = string.Empty;

        foreach(var b in bytes)
        {
            result += hexDigits[(b & 0xf0) >> 4];
            result += hexDigits[b & 0xf];
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
