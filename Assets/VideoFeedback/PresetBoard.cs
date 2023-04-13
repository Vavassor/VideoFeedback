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

    public string selectedOptionPresetCode;

    public SyncedSlider brightnessSlider;
    public VRC_Pickup cameraPickup;
    public SyncedToggle clearCameraToggle;
    public SyncedSlider hueShiftSlider;
    public SyncedToggle mirrorXToggle;
    public SyncedToggle mirrorYToggle;
    public SyncedToggle orthographicProjectionToggle;
    public InputField presetCodeInputField;

    // Data to be saved in a preset code.
    private float brightness;
    private Vector3 cameraPosition;
    private Quaternion cameraRotation;
    private bool clearCamera;
    private float hueShift;
    private bool isProjectionOrthographic;
    private bool mirrorX;
    private bool mirrorY;

    private const string codeId = "VF";
    private const int codeSizeBytes = 33;
    private const ushort currentVersion = 1;
    private const string defaultPresetCode = "VkYAAb0KHgA/2jBUP0akl5X3O/qs2hshP0yKHEEmgbkD";

    public void OnClickGeneratePresetCode()
    {
        brightness = brightnessSlider.value;
        cameraPosition = cameraPickup.transform.position;
        cameraRotation = cameraPickup.transform.rotation;
        clearCamera = clearCameraToggle.isOn;
        hueShift = hueShiftSlider.value;
        isProjectionOrthographic = orthographicProjectionToggle.isOn;
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

    public void OnClickPresetOptionButton()
    {
        DeserializePreset(selectedOptionPresetCode);
        ApplyPreset();
    }

    public void OnClickResetAllButton()
    {
        DeserializePreset(defaultPresetCode);
        ApplyPreset();
    }

    public string SerializePreset()
    {
        byte[] bytes = new byte[codeSizeBytes];
        WriteSByte((sbyte) codeId[0], bytes, 0);
        WriteSByte((sbyte) codeId[1], bytes, 1);
        WriteUInt16(currentVersion, bytes, 2);
        WriteVector3(cameraPosition, bytes, 4);
        WriteHalfQuaternion(cameraRotation, bytes, 16);
        WriteSingle(brightness, bytes, 24);
        WriteSingle(hueShift, bytes, 28);
        bytes[32] = (byte) ((ToInt(mirrorX) << 3) | (ToInt(mirrorY) << 2) | (ToInt(isProjectionOrthographic) << 1) | ToInt(clearCamera));
        return Convert.ToBase64String(bytes);
    }

    private int ToInt(bool value)
    {
        return value ? 1 : 0;
    }

    public bool DeserializePreset(string text)
    {
        var bytes = Convert.FromBase64String(text);

        if (bytes.Length != codeSizeBytes)
        {
            return false;
        }

        var id0 = ReadSByte(bytes, 0);
        var id1 = ReadSByte(bytes, 1);
        var version = ReadUInt16(bytes, 2);

        if (id0 == codeId[0] && id1 == codeId[1] && version != currentVersion)
        {
            return false;
        }

        cameraPosition = ReadVector3(bytes, 4);
        cameraRotation = ReadHalfQuaternion(bytes, 16);
        brightness = ReadSingle(bytes, 24);
        hueShift = ReadSingle(bytes, 28);

        var flags = bytes[32];
        clearCamera = (flags & 1) > 0;
        isProjectionOrthographic = (flags & 2) > 0;
        mirrorY = (flags & 4) > 0;
        mirrorX = (flags & 8) > 0;

        return true;
    }

    private void ApplyPreset()
    {
        brightnessSlider.value = brightness;
        brightnessSlider.OnSetValueExternally();

        Networking.SetOwner(Networking.LocalPlayer, cameraPickup.gameObject);
        cameraPickup.transform.SetPositionAndRotation(cameraPosition, cameraRotation);

        clearCameraToggle.isOn = clearCamera;
        clearCameraToggle.OnSetValueExternally();

        hueShiftSlider.value = hueShift;
        hueShiftSlider.OnSetValueExternally();

        orthographicProjectionToggle.isOn = isProjectionOrthographic;
        orthographicProjectionToggle.OnSetValueExternally();

        mirrorXToggle.isOn = mirrorX;
        mirrorXToggle.OnSetValueExternally();

        mirrorYToggle.isOn = mirrorY;
        mirrorYToggle.OnSetValueExternally();
    }

    public bool ReadBool(byte[] buffer, int index)
    {
        return buffer[index] == 1;
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

    public Vector3 ReadVector3(byte[] buffer, int index)
    {
        float x = ReadSingle(buffer, index);
        index += 4;
        float y = ReadSingle(buffer, index);
        index += 4;
        float z = ReadSingle(buffer, index);

        return new Vector3(x, y, z);
    }

    public int WriteBool(bool value, byte[] buffer, int index)
    {
        if (value) buffer[index] = 1;
        else buffer[index] = 0;
        return 1;
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

    public int WriteVector3(Vector3 value, byte[] buffer, int index)
    {
        WriteSingle(value.x, buffer, index);
        index += 4;
        WriteSingle(value.y, buffer, index);
        index += 4;
        WriteSingle(value.z, buffer, index);
        return 12;
    }
}
