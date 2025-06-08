#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Bounds2DInt))]
public class Bounds2DIntDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty minX = property.FindPropertyRelative("minX");
        SerializedProperty maxX = property.FindPropertyRelative("maxX");
        SerializedProperty minZ = property.FindPropertyRelative("minZ");
        SerializedProperty maxZ = property.FindPropertyRelative("maxZ");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, label);

        // Split line into four columns: Label1, Field1, Label2, Field2
        float labelWidth = 45f;
        float fieldWidth = (position.width - 2 * labelWidth - 10f) / 2;

        Rect row1 = new Rect(position.x, position.y, position.width, lineHeight);
        Rect row2 = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

        // --- X Row ---
        Rect minXLabel = new Rect(row1.x, row1.y, labelWidth, lineHeight);
        Rect minXField = new Rect(minXLabel.xMax + 2, row1.y, fieldWidth, lineHeight);

        Rect maxXLabel = new Rect(minXField.xMax + 6, row1.y, labelWidth, lineHeight);
        Rect maxXField = new Rect(maxXLabel.xMax + 2, row1.y, fieldWidth, lineHeight);

        EditorGUI.LabelField(minXLabel, "Min X");
        minX.intValue = EditorGUI.IntField(minXField, GUIContent.none, minX.intValue);

        EditorGUI.LabelField(maxXLabel, "Max X");
        maxX.intValue = EditorGUI.IntField(maxXField, GUIContent.none, maxX.intValue);

        // --- Z Row ---
        Rect minZLabel = new Rect(row2.x, row2.y, labelWidth, lineHeight);
        Rect minZField = new Rect(minZLabel.xMax + 2, row2.y, fieldWidth, lineHeight);

        Rect maxZLabel = new Rect(minZField.xMax + 6, row2.y, labelWidth, lineHeight);
        Rect maxZField = new Rect(maxZLabel.xMax + 2, row2.y, fieldWidth, lineHeight);

        EditorGUI.LabelField(minZLabel, "Min Z");
        minZ.intValue = EditorGUI.IntField(minZField, GUIContent.none, minZ.intValue);

        EditorGUI.LabelField(maxZLabel, "Max Z");
        maxZ.intValue = EditorGUI.IntField(maxZField, GUIContent.none, maxZ.intValue);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2f) * 2;
    }
}
#endif