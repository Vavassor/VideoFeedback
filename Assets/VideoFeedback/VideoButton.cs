
using UdonSharp;
using UdonSharp.Video;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
using UdonSharpEditor;
#endif

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class VideoButton : UdonSharpBehaviour
{
    public USharpVideoPlayer targetVideoPlayer;
    public VRCUrl[] playlist = new VRCUrl[0];

    Button button;

    void Start()
    {
        button = GetComponentInChildren<Button>();
        UpdateOwnership();

        targetVideoPlayer.RegisterCallbackReceiver(this);
    }

    public void OnButtonPress()
    {
        targetVideoPlayer.PlayVideo(playlist[0]);
    }

    public void OnUSharpVideoLockChange()
    {
        UpdateOwnership();
    }

    public void OnUSharpVideoOwnershipChange()
    {
        UpdateOwnership();
    }

    void UpdateOwnership()
    {
        button.interactable = targetVideoPlayer.CanControlVideoPlayer();
    }
}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
[CustomEditor(typeof(VideoButton))]
public class VideoButtonEditor : Editor
{
    SerializedProperty playerProperty;
    SerializedProperty playlistProperty;

    ReorderableList playlistList;

    private void OnEnable()
    {
        playerProperty = serializedObject.FindProperty(nameof(VideoButton.targetVideoPlayer));
        playlistProperty = serializedObject.FindProperty(nameof(VideoButton.playlist));

        playlistList = new ReorderableList(serializedObject, playlistProperty, true, true, true, true);
        playlistList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            Rect testFieldRect = new Rect(rect.x, rect.y + 2, rect.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(testFieldRect, playlistList.serializedProperty.GetArrayElementAtIndex(index), label: new GUIContent());
        };
        playlistList.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, new GUIContent("URLs", "Only the first URL is used. This is a list due to an inspector bug.")); };
    }

    public override void OnInspectorGUI()
    {
        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
        {
            return;
        }

        EditorGUILayout.PropertyField(playerProperty);
        playlistList.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
