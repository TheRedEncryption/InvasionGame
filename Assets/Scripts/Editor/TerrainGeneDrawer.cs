using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TerrainGene))]
public class TerrainGeneDrawer : PropertyDrawer
{
    private readonly float lineSpace = EditorGUIUtility.singleLineHeight + 2;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // foldout (the ability to expand/collapse the actual TerrainGene itself)
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight),
            property.isExpanded, label, true
        );

        if (property.isExpanded)
        {
            // we are now inside of the property
            EditorGUI.indentLevel++;

            // to keep track of the current y-position in order to cleanly stack UI
            float y = position.y + lineSpace;

            // gene type
            SerializedProperty terrainGeneTypeProperty = property.FindPropertyRelative("terrainGeneType");
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), terrainGeneTypeProperty);
            y += lineSpace;

            // conditional fields
            switch ((TerrainSeed.GeneType)terrainGeneTypeProperty.enumValueIndex)
            {
                case TerrainSeed.GeneType.noise:
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("noiseScale"));
                    y += lineSpace;

                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("heightSteps"));
                    y += lineSpace;
                    break;

                case TerrainSeed.GeneType.plane:
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("planeBounds"), true);
                    y += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("planeBounds"), true) + 2;

                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("planeHeight"));
                    y += lineSpace;
                    break;

                case TerrainSeed.GeneType.island:
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("islandFalloff"));
                    y += lineSpace;
                    break;

                case TerrainSeed.GeneType.none:
                default:
                    EditorGUI.HelpBox(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), "No gene type selected? That was so not sigma of you, you know? Shame...", MessageType.Info);
                    y += lineSpace;
                    break;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (property.isExpanded)
        {
            height += lineSpace; // terrainGeneType enum

            SerializedProperty typeProp = property.FindPropertyRelative("terrainGeneType");
            switch ((TerrainSeed.GeneType)typeProp.enumValueIndex)
            {
                case TerrainSeed.GeneType.noise:
                    height += 2 * (EditorGUIUtility.singleLineHeight + 2);
                    break;

                case TerrainSeed.GeneType.plane:
                    height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("planeBounds"), true) + 2;
                    height += lineSpace;
                    break;

                case TerrainSeed.GeneType.island:
                    height += lineSpace;
                    break;

                case TerrainSeed.GeneType.none:
                default:
                    height += lineSpace;
                    break;
            }
        }

        return height;
    }
}
