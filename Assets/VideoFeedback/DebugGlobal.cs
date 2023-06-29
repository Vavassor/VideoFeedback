
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using System.Globalization;

public class DebugGlobal : UdonSharpBehaviour
{
    void Start()
    {
        Debug.Log("DebugGlobal - Current Culture: " + CultureInfo.CurrentCulture);
    }
}
