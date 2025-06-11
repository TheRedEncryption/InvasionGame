using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(GeneratedReticleUI))]
public class GeneratedReticleUIEditor : Editor
{
    private ReorderableList _list;
    private SerializedProperty _partsProp;
    private ReticlePreset selectedPreset;
    private string newPresetName = "New Reticle Preset";
    private string newPresetFolder = "Assets/Assets/UI";

    // Unique control IDs for picker dialogs
    private const int LoadPresetPickerID = 12345;
    private const int SavePresetPickerID = 23456;

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

        // --- Clear Reticle Button ---
        if (GUILayout.Button("Clear Reticle"))
        {
            if (EditorUtility.DisplayDialog(
                    "Clear Reticle?",
                    "Are you sure you want to remove all reticle elements?",
                    "Clear",
                    "Cancel"
                ))
            {
                _partsProp.ClearArray();
                serializedObject.ApplyModifiedProperties();
                var reticleUI = (GeneratedReticleUI)target;
                reticleUI.SetVerticesDirty();
                EditorUtility.SetDirty(reticleUI);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Reticle Presets", EditorStyles.boldLabel);

        // Display the currently selected preset for reference
        //selectedPreset = (ReticlePreset)EditorGUILayout.ObjectField("Preset Asset", selectedPreset, typeof(ReticlePreset), false);

        // --- Load from Preset Picker ---
        if (GUILayout.Button("Load from Preset"))
        {
            EditorGUIUtility.ShowObjectPicker<ReticlePreset>(selectedPreset, false, "", LoadPresetPickerID);
        }

        // --- Save to Preset Picker ---
        if (GUILayout.Button("Save to Preset"))
        {
            EditorGUIUtility.ShowObjectPicker<ReticlePreset>(selectedPreset, false, "", SavePresetPickerID);
        }

        // --- Handle Object Picker events ---
        if (Event.current.commandName == "ObjectSelectorUpdated")
        {
            int controlID = EditorGUIUtility.GetObjectPickerControlID();
            var pickedPreset = EditorGUIUtility.GetObjectPickerObject() as ReticlePreset;
            if (controlID == LoadPresetPickerID && pickedPreset != null)
            {
                selectedPreset = pickedPreset;
                var reticleUI = (GeneratedReticleUI)target;
                Undo.RecordObject(reticleUI, "Load Reticle Preset");
                ReticlePresetUtility.LoadPreset(reticleUI, selectedPreset);
                serializedObject.Update();
                reticleUI.SetVerticesDirty();
                EditorUtility.SetDirty(reticleUI);
                Repaint();
            }
            else if (controlID == SavePresetPickerID && pickedPreset != null)
            {
                selectedPreset = pickedPreset;
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
                    EditorUtility.SetDirty(selectedPreset);
                }
                Repaint();
            }
        }

        // --- Create New Preset Section ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Create New Preset", EditorStyles.boldLabel);
        newPresetName = EditorGUILayout.TextField("Preset Name", newPresetName);
        newPresetFolder = EditorGUILayout.TextField("Folder", newPresetFolder);

        if (GUILayout.Button("Save As New Preset"))
        {
            string safeName = newPresetName.Trim();
            if (string.IsNullOrEmpty(safeName)) safeName = "New Reticle Preset";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{newPresetFolder}/{safeName}.asset");

            var newPreset = ScriptableObject.CreateInstance<ReticlePreset>();
            ReticlePresetUtility.SavePreset((GeneratedReticleUI)target, newPreset);

            AssetDatabase.CreateAsset(newPreset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newPreset;
            selectedPreset = newPreset;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
