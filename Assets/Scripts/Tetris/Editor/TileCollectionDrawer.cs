using UnityEngine;
using UnityEditor;
using System;

[CustomPropertyDrawer(typeof(TileCollection))]
public class TileCollectionDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var tileProps = property.FindPropertyRelative("tiles");
        var enumNames = Enum.GetNames(typeof(Role));
        if (tileProps.arraySize != enumNames.Length)
        {
            tileProps.arraySize = enumNames.Length;
        }
        EditorGUI.BeginProperty(position, label, property);
        position.height = EditorGUIUtility.singleLineHeight;
        for (var i = 0; i < tileProps.arraySize; i++)
        {
            var eleProp = tileProps.GetArrayElementAtIndex(i);
            EditorGUI.PropertyField(position, eleProp, new GUIContent(enumNames[i]));
            position.y += EditorGUIUtility.singleLineHeight + 2;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2) * 4 + 2;
    }
}
