
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LocalizedText : UdonSharpBehaviour
{
    public string key;
    public bool shouldTransformUppercase;

    private Text textComponent;

    void Start()
    {
        textComponent = GetComponent<Text>();
    }

    public void SetText(string text)
    {
        if (shouldTransformUppercase)
        {
            text = text.ToUpper();
        }

        textComponent.text = text;
    }
}
