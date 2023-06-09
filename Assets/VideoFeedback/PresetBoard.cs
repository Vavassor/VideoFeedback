﻿// The read and write functions are from UNet by Xytabich. https://github.com/Xytabich/UNet
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

    public string selectedOptionPresetCode;

    public SyncedSlider brightnessSlider;
    public VRC_Pickup cameraPickup;
    public SyncedSlider chromaticDistortionSlider;
    public SyncedToggle clearCameraToggle;
    public ColorButton clearColorButton;
    public SyncedSlider contrastSlider;
    public SyncedSlider edgeBrightnessSlider;
    public SyncedSlider fieldOfViewSlider;
    public SyncedSlider flowDistortionSlider;
    public ColorButton gradientMappingStop0ColorButton;
    public ColorButton gradientMappingStop1ColorButton;
    public SyncedToggle gradientMappingToggle;
    public SyncedSlider hueShiftSlider;
    public SyncedToggle invertColorToggle;
    public SyncedToggle isStabilizerEnabledToggle;
    public SyncedToggle isVerticalScreenToggle;
    public SyncedSlider mirrorTileCountSlider;
    public SyncedToggle mirrorXToggle;
    public SyncedToggle mirrorYToggle;
    public SyncedToggle orthographicProjectionToggle;
    public SyncedSlider saturationSlider;
    public SyncedSlider sharpnessSlider;
    public SyncedSlider smoothingFramesSlider;
    public InputField presetCodeInputField;
    public GameObject loadPresetCodeError;
    public Stabilizer stabilizer;

    // Data to be saved in a preset code.
    private float brightness;
    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private float chromaticDistortion;
    private Color clearColor;
    private float contrast;
    private float edgeBrightness;
    private float fieldOfView;
    private float flowDistortion;
    private Color gradientMappingStop0Color;
    private Color gradientMappingStop1Color;
    private bool clearCamera;
    private float hueShift;
    private bool invertColor;
    private bool isProjectionOrthographic;
    private bool isVerticalScreen;
    private int mirrorTileCount;
    private bool mirrorX;
    private bool mirrorY;
    private float saturation;
    private float sharpness;
    private bool useGradientMapping;

    private const string codeId = "VF";
    private const int codeSizeBytes = 46;
    private const int headerSizeBytes = 4;
    private const ushort currentVersion = 5;
    private const string defaultPresetCode = "VkYABL8g4A4/v5PVPsWoCUhZWVddiTwAOtDJ4wAA0c1zRAUuAAAAAD0BAAAAAA==";
    private float defaultFieldOfView = 60.0f;
    private Color defaultClearColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);
    private Color defaultStop0Color = new Color(0.4470588f, 0.8078431f, 0.8196079f);
    private Color defaultStop1Color = new Color(0.1844448f, 0.02889f, 0.27f);

    public void OnClickGeneratePresetCode()
    {
        loadPresetCodeError.SetActive(false);

        brightness = brightnessSlider.value;
        cameraPosition = cameraPickup.transform.position;
        cameraRotation = cameraPickup.transform.rotation;
        clearCamera = clearCameraToggle.isOn;
        chromaticDistortion = chromaticDistortionSlider.value;

        clearColorButton.OnGetColor();
        clearColor = clearColorButton.color;

        contrast = contrastSlider.value;
        edgeBrightness = edgeBrightnessSlider.value;
        fieldOfView = fieldOfViewSlider.value;
        flowDistortion = flowDistortionSlider.value;

        gradientMappingStop0ColorButton.OnGetColor();
        gradientMappingStop0Color = gradientMappingStop0ColorButton.color;

        gradientMappingStop1ColorButton.OnGetColor();
        gradientMappingStop1Color = gradientMappingStop1ColorButton.color;

        hueShift = hueShiftSlider.value;
        invertColor = invertColorToggle.isOn;
        isProjectionOrthographic = orthographicProjectionToggle.isOn;
        isVerticalScreen = isVerticalScreenToggle.isOn;
        mirrorTileCount = (int) mirrorTileCountSlider.value;
        mirrorX = mirrorXToggle.isOn;
        mirrorY = mirrorYToggle.isOn;
        saturation = saturationSlider.value;
        sharpness = sharpnessSlider.value;
        useGradientMapping = gradientMappingToggle.isOn;

        presetCodeInputField.text = SerializePreset();
    }

    public void OnClickLoadPresetCode()
    {
        loadPresetCodeError.SetActive(false);

        var didDeserializeSucceed = DeserializePreset(StripWhitespace(presetCodeInputField.text));
        if (didDeserializeSucceed)
        {
            ApplyPreset();
        }
        else
        {
            loadPresetCodeError.SetActive(true);
        }
    }

    public void OnClickPresetOptionButton()
    {
        DeserializePreset(selectedOptionPresetCode);
        ApplyPreset();
    }

    public void OnClickResetAllButton()
    {
        DeserializePreset(defaultPresetCode);
        ApplyPreset();

        // Reset fields that aren't saved in presets, to help players who get confused.
        isStabilizerEnabledToggle.isOn = true;
        isStabilizerEnabledToggle.OnSetValueExternally();

        smoothingFramesSlider.value = 6;
        smoothingFramesSlider.OnSetValueExternally();
    }

    public string SerializePreset()
    {
        byte[] bytes = new byte[codeSizeBytes];
        WriteSByte((sbyte) codeId[0], bytes, 0);
        WriteSByte((sbyte) codeId[1], bytes, 1);
        WriteUInt16(currentVersion, bytes, 2);
        WriteVector3(cameraPosition, bytes, 4);
        WriteHalfVector3(cameraRotation.eulerAngles, bytes, 16);
        WriteHalf(contrast, bytes, 22);
        WriteHalf(brightness, bytes, 24);
        WriteHalf(hueShift, bytes, 26);
        WriteHalf(chromaticDistortion, bytes, 28);
        WriteOpaqueColor(gradientMappingStop0Color, bytes, 30);
        WriteOpaqueColor(gradientMappingStop1Color, bytes, 33);
        WriteOpaqueColor(clearColor, bytes, 36);
        WriteUnorm(edgeBrightness, bytes, 39);
        bytes[40] = (byte) (((((int) Mathf.Floor(fieldOfView) - 45) & 0x3f) << 2) | mirrorTileCount);
        bytes[41] = (byte) ((ToInt(isVerticalScreen) << 6) | (ToInt(useGradientMapping) << 5) | (ToInt(invertColor) << 4) | (ToInt(mirrorX) << 3) | (ToInt(mirrorY) << 2) | (ToInt(isProjectionOrthographic) << 1) | ToInt(clearCamera));
        WriteUnorm(4.0f * sharpness, bytes, 42);
        bytes[43] = (byte) flowDistortion;
        WriteHalf(saturation, bytes, 44);

        return Convert.ToBase64String(bytes);
    }

    private int ToInt(bool value)
    {
        return value ? 1 : 0;
    }

    private bool IsBase64Char(char digit)
    {
        if (digit >= 'A' && digit <= 'Z') return true;
        if (digit >= 'a' && digit <= 'z') return true;
        if (digit >= '0' && digit <= '9') return true;
        if (digit == '+') return true;
        if (digit == '/') return true;
        return false;
    }

    private bool IsBase64(string text)
    {
        if (text.Length % 4 != 0)
        {
            return false;
        }

        for (var i = 0; i < text.Length; i++)
        {
            if (!IsBase64Char(text[i]))
            {
                if (text[i] == '=')
                {
                    // Padding characters are only valid at the end as the last one or two characters.
                    return i == text.Length - 1 || (text[i + 1] == '=' && i == text.Length - 2);
                }

                return false;
            }
        }

        return true;
    }

    private string StripWhitespace(string text)
    {
        return string.Join("", text.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }

    public bool DeserializePreset(string text)
    {
        // There's no try/catch in U#. So in order to prevent invalid preset codes from crashing
        // the UdonBehaviour, validate that it's actually base64.
        if (!IsBase64(text))
        {
            return false;
        }

        var bytes = Convert.FromBase64String(text);

        if (bytes.Length < headerSizeBytes)
        {
            return false;
        }

        var id0 = ReadSByte(bytes, 0);
        var id1 = ReadSByte(bytes, 1);
        var version = ReadUInt16(bytes, 2);

        if (id0 != codeId[0] || id1 != codeId[1] || version > currentVersion)
        {
            return false;
        }

        if (version < currentVersion)
        {
            bytes = MigrateVersions(bytes, version);
        }

        if (bytes.Length != codeSizeBytes)
        {
            return false;
        }

        cameraPosition = ReadVector3(bytes, 4);
        cameraRotation = Quaternion.Euler(ReadHalfVector3(bytes, 16));
        contrast = ReadHalf(bytes, 22);
        brightness = ReadHalf(bytes, 24);
        hueShift = ReadHalf(bytes, 26);
        chromaticDistortion = ReadHalf(bytes, 28);
        gradientMappingStop0Color = ReadOpaqueColor(bytes, 30);
        gradientMappingStop1Color = ReadOpaqueColor(bytes, 33);
        clearColor = ReadOpaqueColor(bytes, 36);
        edgeBrightness = ReadUnorm(bytes, 39);

        var mirrorTileAndFov = bytes[40];
        mirrorTileCount = mirrorTileAndFov & 0x3;
        fieldOfView = ((mirrorTileAndFov & 0xfc) >> 2) + 45;

        var flags = bytes[41];
        clearCamera = (flags & 0x01) > 0;
        isProjectionOrthographic = (flags & 0x02) > 0;
        mirrorY = (flags & 0x04) > 0;
        mirrorX = (flags & 0x08) > 0;
        invertColor = (flags & 0x10) > 0;
        useGradientMapping = (flags & 0x20) > 0;
        isVerticalScreen = (flags & 0x40) > 0;

        sharpness = 0.25f * ReadUnorm(bytes, 42);
        flowDistortion = bytes[43];
        saturation = ReadHalf(bytes, 44);

        return true;
    }

    private void ApplyPreset()
    {
        brightnessSlider.value = brightness;
        brightnessSlider.OnSetValueExternally();

        Networking.SetOwner(Networking.LocalPlayer, cameraPickup.gameObject);
        cameraPickup.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
        stabilizer.OnTeleport();

        chromaticDistortionSlider.value = chromaticDistortion;
        chromaticDistortionSlider.OnSetValueExternally();

        clearCameraToggle.isOn = clearCamera;
        clearCameraToggle.OnSetValueExternally();

        clearColorButton.color = clearColor;
        clearColorButton.OnSetValueExternally();

        contrastSlider.value = contrast;
        contrastSlider.OnSetValueExternally();

        edgeBrightnessSlider.value = edgeBrightness;
        edgeBrightnessSlider.OnSetValueExternally();

        fieldOfViewSlider.value = fieldOfView;
        fieldOfViewSlider.OnSetValueExternally();

        flowDistortionSlider.value = flowDistortion;
        flowDistortionSlider.OnSetValueExternally();

        gradientMappingStop0ColorButton.color = gradientMappingStop0Color;
        gradientMappingStop0ColorButton.OnSetValueExternally();

        gradientMappingStop1ColorButton.color = gradientMappingStop1Color;
        gradientMappingStop1ColorButton.OnSetValueExternally();

        hueShiftSlider.value = hueShift;
        hueShiftSlider.OnSetValueExternally();

        invertColorToggle.isOn = invertColor;
        invertColorToggle.OnSetValueExternally();

        isVerticalScreenToggle.isOn = isVerticalScreen;
        isVerticalScreenToggle.OnSetValueExternally();

        orthographicProjectionToggle.isOn = isProjectionOrthographic;
        orthographicProjectionToggle.OnSetValueExternally();

        mirrorTileCountSlider.value = mirrorTileCount;
        mirrorTileCountSlider.OnSetValueExternally();

        mirrorXToggle.isOn = mirrorX;
        mirrorXToggle.OnSetValueExternally();

        mirrorYToggle.isOn = mirrorY;
        mirrorYToggle.OnSetValueExternally();

        saturationSlider.value = saturation;
        saturationSlider.OnSetValueExternally();

        sharpnessSlider.value = sharpness;
        sharpnessSlider.OnSetValueExternally();

        gradientMappingToggle.isOn = useGradientMapping;
        gradientMappingToggle.OnSetValueExternally();
    }

    public float ReadHalf(byte[] buffer, int index)
    {
        return Mathf.HalfToFloat(ReadUInt16(buffer, index));
    }

    public Quaternion ReadHalfQuaternion(byte[] buffer, int index)
    {
        float x = ReadHalf(buffer, index);
        index += 2;
        float y = ReadHalf(buffer, index);
        index += 2;
        float z = ReadHalf(buffer, index);
        index += 2;
        float w = ReadHalf(buffer, index);

        return new Quaternion(x, y, z, w);
    }

    public Vector3 ReadHalfVector3(byte[] buffer, int index)
    {
        float x = ReadHalf(buffer, index);
        index += 2;
        float y = ReadHalf(buffer, index);
        index += 2;
        float z = ReadHalf(buffer, index);

        return new Vector3(x, y, z);
    }

    public Color ReadOpaqueColor(byte[] buffer, int index)
    {
        float b = ReadUnorm(buffer, index);
        index++;
        float g = ReadUnorm(buffer, index);
        index++;
        float r = ReadUnorm(buffer, index);
        return new Color(r, g, b);
    }

    public sbyte ReadSByte(byte[] buffer, int index)
    {
        int value = buffer[index];
        if (value >= 0x80) value = value - 256;
        return Convert.ToSByte(value);
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

    public ushort ReadUInt16(byte[] buffer, int index)
    {
        return Convert.ToUInt16(buffer[index] << BIT8 | buffer[index + 1]);
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

    public float ReadUnorm(byte[] buffer, int index)
    {
        float value = buffer[index] / 255.0f;
        return value;
    }

    public Vector3 ReadVector3(byte[] buffer, int index)
    {
        float x = ReadSingle(buffer, index);
        index += 4;
        float y = ReadSingle(buffer, index);
        index += 4;
        float z = ReadSingle(buffer, index);

        return new Vector3(x, y, z);
    }

    public int WriteHalf(float value, byte[] buffer, int index)
    {
        return WriteUInt16(Mathf.FloatToHalf(value), buffer, index);
    }

    public int WriteHalfQuaternion(Quaternion value, byte[] buffer, int index)
    {
        WriteHalf(value.x, buffer, index);
        index += 2;
        WriteHalf(value.y, buffer, index);
        index += 2;
        WriteHalf(value.z, buffer, index);
        index += 2;
        WriteHalf(value.w, buffer, index);
        return 8;
    }

    public int WriteHalfVector3(Vector3 value, byte[] buffer, int index)
    {
        WriteHalf(value.x, buffer, index);
        index += 2;
        WriteHalf(value.y, buffer, index);
        index += 2;
        WriteHalf(value.z, buffer, index);
        return 6;
    }

    public int WriteOpaqueColor(Color color, byte[] buffer, int index)
    {
        WriteUnorm(color.b, buffer, index);
        index++;
        WriteUnorm(color.g, buffer, index);
        index++;
        WriteUnorm(color.r, buffer, index);
        return 3;
    }

    public int WriteSByte(sbyte value, byte[] buffer, int index)
    {
        buffer[index] = (byte)(value < 0 ? (value + 256) : value);
        return 1;
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

    public int WriteUInt16(ushort value, byte[] buffer, int index)
    {
        int tmp = Convert.ToInt32(value);
        buffer[index] = (byte)(tmp >> BIT8);
        index++;
        buffer[index] = (byte)(tmp & 0xFF);
        return 2;
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

    public int WriteUnorm(float value, byte[] buffer, int index)
    {
        buffer[index] = (byte) (255.0f * value);
        return 1;
    }

    public int WriteVector3(Vector3 value, byte[] buffer, int index)
    {
        WriteSingle(value.x, buffer, index);
        index += 4;
        WriteSingle(value.y, buffer, index);
        index += 4;
        WriteSingle(value.z, buffer, index);
        return 12;
    }

    private byte[] MigrateVersions(byte[] bytes, int version)
    {
        if (version == 1)
        {
            bytes = MigrateVersion1To2(bytes);
            version = 2;
        }

        if (version == 2)
        {
            bytes = MigrateVersion2To3(bytes);
            version = 3;
        }

        if (version == 3)
        {
            bytes = MigrateVersion3To4(bytes);
            version = 4;
        }

        if (version == 4)
        {
            bytes = MigrateVersion4To5(bytes);
            version = 5;
        }

        return bytes;
    }

    private byte[] MigrateVersion1To2(byte[] bytes)
    {
        byte[] newBytes = new byte[codeSizeBytes];

        WriteSByte((sbyte) codeId[0], bytes, 0);
        WriteSByte((sbyte) codeId[1], bytes, 1);
        WriteUInt16(2, bytes, 2);
        CopyBytes(bytes, 4, newBytes, 4, 20);
        WriteHalf(ReadSingle(bytes, 24), newBytes, 24);
        WriteHalf(ReadSingle(bytes, 28), newBytes, 26);
        WriteOpaqueColor(defaultStop0Color, newBytes, 30);
        WriteOpaqueColor(defaultStop1Color, newBytes, 33);
        newBytes[37] = 1;
        newBytes[38] = bytes[32];

        return newBytes;
    }

    private byte[] MigrateVersion2To3(byte[] bytes)
    {
        byte[] newBytes = new byte[codeSizeBytes];

        WriteSByte((sbyte)codeId[0], bytes, 0);
        WriteSByte((sbyte)codeId[1], bytes, 1);
        WriteUInt16(3, bytes, 2);
        CopyBytes(bytes, 4, newBytes, 4, 32);
        WriteOpaqueColor(defaultClearColor, newBytes, 36);
        newBytes[39] = bytes[36];
        newBytes[40] = (byte) (bytes[37] | (((int) Mathf.Floor(defaultFieldOfView) - 45) << 2));
        newBytes[41] = bytes[38];

        return newBytes;
    }

    private byte[] MigrateVersion3To4(byte[] bytes)
    {
        byte[] newBytes = new byte[codeSizeBytes];

        WriteSByte((sbyte)codeId[0], bytes, 0);
        WriteSByte((sbyte)codeId[1], bytes, 1);
        WriteUInt16(4, bytes, 2);
        CopyBytes(bytes, 4, newBytes, 4, 12);
        WriteHalfVector3(ReadHalfQuaternion(bytes, 16).eulerAngles, newBytes, 16);
        WriteHalf(1.0f, newBytes, 22);
        CopyBytes(bytes, 24, newBytes, 24, 18);

        return newBytes;
    }

    private byte[] MigrateVersion4To5(byte[] bytes)
    {
        byte[] newBytes = new byte[codeSizeBytes];

        CopyBytes(bytes, 0, newBytes, 0, 46);
        WriteUInt16(5, bytes, 2);

        return newBytes;
    }

    private void CopyBytes(byte[] from, int fromIndex, byte[] to, int toIndex, int size)
    {
        for (var i = 0; i < size; i++)
        {
            to[toIndex + i] = from[fromIndex + i];
        }
    }
}
