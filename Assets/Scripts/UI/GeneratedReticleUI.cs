using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class GeneratedReticleUI : MaskableGraphic
{
    [SerializeReference]
    private List<ReticlePart> reticleParts = new List<ReticlePart>();
    public List<ReticlePart> ReticleParts
    {
        get => reticleParts;
        set
        {
            reticleParts = value;
            SetVerticesDirty();
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (reticleParts == null) return;

        foreach (var part in reticleParts)
        {
            if (part == null) continue;
            part.Draw(vh);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
