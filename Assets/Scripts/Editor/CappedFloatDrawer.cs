#if UNITY_EDITOR
using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomPropertyDrawer(typeof(CappedFloat))]
public class CappedFloatDrawer : PropertyDrawer
{
    // How to Program:
    // 1. Lock at someone else's code
    // 2. Plagarism :D
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        CappedFloat target = fieldInfo.GetValue(SerializedPropertyUtils.GetParentObject(property)) as CappedFloat;

        // SerializedProperty currValue = property.FindPropertyRelative("CurrValue");
        // SerializedProperty maxValue = property.FindPropertyRelative("MaxValue");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = 2f;

        EditorGUI.BeginProperty(position, label, property);

        position = EditorGUI.PrefixLabel(position, label);

        float labelWidth = 45f;
        float fieldWidth = position.width - EditorGUIUtility.fieldWidth + 5;

        Rect row1 = new(position.x, position.y, position.width, lineHeight);
        Rect row2 = new(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

        Rect currValLabel = new(row1.x, row1.y, labelWidth, lineHeight);
        Rect currValField = new(currValLabel.xMax, row1.y, fieldWidth, lineHeight);

        Rect maxValLabel = new(row2.x, row2.y, labelWidth, lineHeight);
        Rect maxValField = new(maxValLabel.xMax, row2.y, fieldWidth, lineHeight);

        EditorGUI.LabelField(maxValLabel, "Max");
        target.MaxValue = EditorGUI.FloatField(maxValField, GUIContent.none, target.MaxValue);

        EditorGUI.LabelField(currValLabel, "Initial");
        target.CurrValue = EditorGUI.Slider(currValField, GUIContent.none, target.CurrValue, 0, target.MaxValue);

        //EditorUtility.SetDirty(property.ob);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return (EditorGUIUtility.singleLineHeight + 2f) * 2;
    }
}
#endif

// JESUS FUCKING CHRIST THANK YOU omidrezat81 YOU HAVE SAVED ME SO HARD RN
// https://discussions.unity.com/t/a-simple-way-to-access-the-class-from-the-property-drawer/900422/5
public static class SerializedPropertyUtils
{
    private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// Retrieves the parent as an object from the specified SerializedProperty.
    /// </summary>
    /// <param name="prop">The SerializedProperty to get the parent object from.</param>
    /// <returns>
    /// The parent object of the specified SerializedProperty. If the property is a top-level field,
    /// it returns the target object of the serializedObject.
    /// </returns>
    public static object GetParentObject(this SerializedProperty prop)
    {
        var path = prop.propertyPath;
        var lastDot = path.LastIndexOf('.');
        if (lastDot == -1)
        {
            return prop.serializedObject.targetObject;
        }
        var parentPath = path[..lastDot];
        return GetObjectFromPropertyPath(prop.serializedObject.targetObject, parentPath);
    }


    private static object GetObjectFromPropertyPath(object root, string path)
    {
        var elements = path.Replace(".Array.data[", "[").Split('.');
        var obj = root;

        foreach (var e in elements)
        {
            if (e.Contains("["))
            {
                // handle arrays
                var eName = e[..e.IndexOf("[", StringComparison.Ordinal)];
                if (e.IndexOf("[", StringComparison.Ordinal) < 0 ||
                    e.IndexOf("[", StringComparison.Ordinal) > e.Length)
                    continue;

                var i = Convert.ToInt32(e[e.IndexOf("[", StringComparison.Ordinal)..].Replace("[", "").Replace("]", ""));

                obj = GetMemberValue(obj, eName);
                if (obj is IList l && i < l.Count)
                {
                    obj = l[i];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                obj = GetMemberValue(obj, e);
                if (obj == null)
                    return null;
            }
        }

        return obj;
    }

    private static object GetMemberValue(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        var f = type.GetField(name, FLAGS);
        if (f != null) return f.GetValue(source);

        var p = type.GetProperty(name, FLAGS);
        return p != null ? p.GetValue(source, null) : null;
    }
}
