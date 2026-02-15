using UnityEditor;
using UnityEngine;

namespace LevelStreaming.Editor
{
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty sceneAssetProp = property.FindPropertyRelative("sceneAsset");
            SerializedProperty scenePathProp = property.FindPropertyRelative("scenePath");

            EditorGUI.BeginChangeCheck();
            var newScene = EditorGUI.ObjectField(position, label, sceneAssetProp.objectReferenceValue, typeof(SceneAsset), false) as SceneAsset;
            if (EditorGUI.EndChangeCheck())
            {
                sceneAssetProp.objectReferenceValue = newScene;
                scenePathProp.stringValue = newScene != null ? AssetDatabase.GetAssetPath(newScene) : string.Empty;
            }

            EditorGUI.EndProperty();
        }
    }
}