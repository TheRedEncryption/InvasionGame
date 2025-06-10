using System;
using System.Linq;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(GeneratedReticleUI))]
public class GeneratedReticleUIEditor : Editor
{
    private ReorderableList _list;
    private SerializedProperty _partsProp;
    private ReticlePreset selectedPreset;

    private void OnEnable()
    {
        _partsProp = serializedObject.FindProperty("reticleParts");
        _list = new ReorderableList(serializedObject, _partsProp, true, true, true, true);

        _list.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Reticle Parts", EditorStyles.boldLabel);

        _list.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            var elem = _partsProp.GetArrayElementAtIndex(index);
            rect.y += 2;
            EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(elem, true)),
                elem,
                includeChildren: true
            );
        };

        _list.elementHeightCallback = index =>
        {
            var elem = _partsProp.GetArrayElementAtIndex(index);
            return EditorGUI.GetPropertyHeight(elem, true) + 8;
        };

        _list.onAddDropdownCallback = (buttonRect, list) =>
        {
            var menu = new GenericMenu();
            // find all non-abstract ReticlePart subclasses
            var types = TypeCache.GetTypesDerivedFrom<ReticlePart>()
                                 .Where(t => !t.IsAbstract);

            foreach (var t in types)
            {
                menu.AddItem(new GUIContent(t.Name), false, () =>
                {
                    _partsProp.arraySize++;
                    var newElem = _partsProp.GetArrayElementAtIndex(_partsProp.arraySize - 1);
                    var inst = (ReticlePart)Activator.CreateInstance(t);
                    newElem.managedReferenceValue = inst;
                    serializedObject.ApplyModifiedProperties();
                });
            }

            menu.DropDown(buttonRect);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        _list.DoLayoutList();

        // --- NEW: Clear Reticle Button ---
        if (GUILayout.Button("Clear Reticle"))
        {
            // Clear the list
            _partsProp.ClearArray();
            serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Reticle Presets", EditorStyles.boldLabel);

        selectedPreset = (ReticlePreset)EditorGUILayout.ObjectField("Preset Asset", selectedPreset, typeof(ReticlePreset), false);

        using (new EditorGUI.DisabledScope(selectedPreset == null))
        {
            if (GUILayout.Button("Load from Preset"))
            {
                var reticleUI = (GeneratedReticleUI)target;
                Undo.RecordObject(reticleUI, "Load Reticle Preset");
                ReticlePresetUtility.LoadPreset(reticleUI, selectedPreset);
                serializedObject.Update();
                reticleUI.SetVerticesDirty();
                EditorUtility.SetDirty(reticleUI);
            }
            if (GUILayout.Button("Save to Preset"))
            {
                if (selectedPreset != null)
                {
                    bool proceed = true;
                    if (selectedPreset.reticleParts != null && selectedPreset.reticleParts.Count > 0)
                    {
                        proceed = EditorUtility.DisplayDialog(
                            "Overwrite Preset?",
                            "This preset already contains a reticle layout. Overwrite?",
                            "Overwrite",
                            "Cancel"
                        );
                    }
                    if (proceed)
                    {
                        ReticlePresetUtility.SavePreset((GeneratedReticleUI)target, selectedPreset);
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }


}
