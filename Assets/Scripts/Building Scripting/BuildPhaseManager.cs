using UnityEngine;
using System;

public class BuildPhaseManager : MonoBehaviour
{
    public enum BuildPhaseTool
    {
        hand,
        place,
        erase
    }

    // where entity IDs are born
    public enum BuildPhaseEntity
    {
        jammyWalker,
        evilJammyWalker,
        timmyStalker
    }

    public BuildPhaseTool currentTool;
    public BuildPhaseEntity currentEntity;

    public EnumToEntityMapping mappingToUse;
    private ObjectPlacer objectPlacer;

    void Start()
    {
        currentTool = BuildPhaseTool.hand;
        currentEntity = BuildPhaseEntity.jammyWalker;
        objectPlacer = FindFirstObjectByType<ObjectPlacer>();
    }

    void Update()
    {

    }

    // The reason that these are integer is because the OnClick of the buttons arent able to set values that arent bools, strings, ints, etc.
    public void SetBuildPhaseTool(int bpt)
    {
        currentTool = (BuildPhaseTool)bpt;
        Debug.Log("Current tool set to " + currentTool);
    }

    public void SetBuildPhaseEntity(int bpe)
    {
        if (bpe > Enum.GetNames(typeof(BuildPhaseEntity)).Length || bpe < 0)
        {
            Debug.LogWarning($"Entity ID {bpe} does not exist!");
            return;
        }
        if (bpe > mappingToUse.mapping.Length || mappingToUse.mapping[bpe] == null)
        {
            Debug.LogWarning($"ENTITY NOT DEFINED IN mappingToUse FOR THIS ENTITY ID: {bpe}!");
            return;
        }

        currentEntity = (BuildPhaseEntity)bpe;
        Debug.Log("Current entity set to " + currentEntity);
        objectPlacer._selectedGameObject = mappingToUse.mapping[bpe];
    }
}
