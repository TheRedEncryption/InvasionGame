using UnityEditor;
using UnityEngine;

public static class ReticlePresetUtility
{
    // Helper: Deep-copy a ReticlePart using ScriptableObject serialization
    public static ReticlePart ClonePart(ReticlePart source)
    {
        // Use a hidden ScriptableObject for deep copy
        var temp = ScriptableObject.CreateInstance<ReticlePartHolder>();
        temp.part = source;
        var temp2 = ScriptableObject.CreateInstance<ReticlePartHolder>();
        EditorUtility.CopySerialized(temp, temp2);
        var clone = temp2.part;
        Object.DestroyImmediate(temp);
        Object.DestroyImmediate(temp2);
        return clone;
    }

    public static void SavePreset(GeneratedReticleUI reticle, ReticlePreset preset)
    {
        preset.reticleParts.Clear();
        foreach (var part in reticle.ReticleParts)
        {
            preset.reticleParts.Add(ClonePart(part));
        }
        EditorUtility.SetDirty(preset);
        AssetDatabase.SaveAssets();
    }

    public static void LoadPreset(GeneratedReticleUI reticle, ReticlePreset preset)
    {
        reticle.ReticleParts.Clear();
        foreach (var part in preset.reticleParts)
        {
            reticle.ReticleParts.Add(ClonePart(part));
        }
        EditorUtility.SetDirty(reticle);
    }
}

// Used to serialize any ReticlePart with Unity's serialization
[System.Serializable]
public class ReticlePartHolder : ScriptableObject
{
    [SerializeReference]
    public ReticlePart part;
}
