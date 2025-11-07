using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(System.Enum), true)]
public class FlagsEnumDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Vérifie si le champ est décoré avec [Flags]
        var type = fieldInfo.FieldType;
        var flagsAttribute = System.Attribute.GetCustomAttribute(type, typeof(System.FlagsAttribute));

        if (flagsAttribute == null)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);

        // Affiche toutes les options sous forme de cases
        property.intValue = EditorGUI.MaskField(
            position,
            label,
            property.intValue,
            property.enumDisplayNames
        );

        EditorGUI.EndProperty();
    }
}