#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Grid))]
public class GridDrawer : PropertyDrawer
{
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Grid target = fieldInfo.GetValue(property.GetParentObject()) as Grid;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, label);

        // Split line into four columns: Label1, Field1, Label2, Field2
        float labelWidth = 45f;
        float fieldWidth = position.width - EditorGUIUtility.fieldWidth + 5;

        Rect row1 = new(position.x, position.y, position.width, lineHeight);
        Rect row2 = new(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

        Rect scaleLabel = new(row1.x, row1.y, labelWidth, lineHeight);
        Rect scaleField = new(scaleLabel.xMax + 2, row1.y, fieldWidth, lineHeight);

        Rect dimensionsLabel = new(row2.x, row2.y, labelWidth, lineHeight);
        Rect dimensionsField = new(scaleLabel.xMax + 2, row2.y, fieldWidth, lineHeight);

        EditorGUI.LabelField(scaleLabel, "Scale");
        target.Scale = EditorGUI.Vector3Field(scaleField, GUIContent.none, target.Scale);

        EditorGUI.LabelField(dimensionsLabel, "Dimensions");
        Vector3Int bridge = EditorGUI.Vector3IntField(dimensionsField, GUIContent.none, new Vector3Int(target.Dimensions.X, target.Dimensions.Y, target.Dimensions.Z));
        target.Dimensions = (Grid.Point)new Vector3(Mathf.Max(bridge.x, 1), Mathf.Max(bridge.y, 1), Mathf.Max(1, bridge.z));

        // Check if the property changed
        if (EditorGUI.EndChangeCheck())
        {
            // Get the target object
            GameObject targetObj = (property.serializedObject.targetObject as MonoBehaviour).gameObject;

            // Mark the GameObject as dirty
            EditorUtility.SetDirty(targetObj);

            // Repaint the Scene view
            SceneView.RepaintAll();
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2f) * 2;
    }
}

[CustomPropertyDrawer(typeof(PlacementGrid))]
public class PlacementGridDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        PlacementGrid target = fieldInfo.GetValue(property.GetParentObject()) as PlacementGrid;

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, label);

        // Split line into four columns: Label1, Field1, Label2, Field2
        float labelWidth = 45f;
        float fieldWidth = position.width - EditorGUIUtility.fieldWidth + 5;

        Rect row1 = new(position.x, position.y, position.width, lineHeight);
        Rect row2 = new(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

        Rect scaleLabel = new(row1.x, row1.y, labelWidth, lineHeight);
        Rect scaleField = new(scaleLabel.xMax + 2, row1.y, fieldWidth, lineHeight);

        Rect dimensionsLabel = new(row2.x, row2.y, labelWidth, lineHeight);
        Rect dimensionsField = new(scaleLabel.xMax + 2, row2.y, fieldWidth, lineHeight);

        EditorGUI.LabelField(scaleLabel, "Scale");
        float scaleFactor = EditorGUI.FloatField(scaleField, GUIContent.none, target.Scale.x);
        target.Scale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        EditorGUI.LabelField(dimensionsLabel, "Dimensions");
        Vector3Int bridge = EditorGUI.Vector3IntField(dimensionsField, GUIContent.none, new Vector3Int(target.Dimensions.X, target.Dimensions.Y, target.Dimensions.Z));
        target.Dimensions = (Grid.Point)new Vector3(Mathf.Max(bridge.x, 1), Mathf.Max(bridge.y, 1), Mathf.Max(1, bridge.z));

        // Check if the property changed
        if (EditorGUI.EndChangeCheck())
        {
            // Get the target object
            GameObject targetObj = (property.serializedObject.targetObject as MonoBehaviour).gameObject;

            // Mark the GameObject as dirty
            EditorUtility.SetDirty(targetObj);

            // Repaint the Scene view
            SceneView.RepaintAll();
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2f) * 2;
    }
}

#endif