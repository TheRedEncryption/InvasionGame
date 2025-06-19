using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Reticle/Reticle Preset", fileName = "ReticlePreset")]
public class ReticlePreset : ScriptableObject
{
    [SerializeReference]
    public List<ReticlePart> reticleParts = new List<ReticlePart>();
}
