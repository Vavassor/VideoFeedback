#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UnityEditor;
using UdonSharpEditor;
using UnityEditorInternal;
using UnityEngine;

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
#endif // !COMPILER_UDONSHARP && UNITY_EDITOR
