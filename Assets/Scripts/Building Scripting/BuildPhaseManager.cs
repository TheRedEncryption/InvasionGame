using UnityEngine;

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

    void Start()
    {
        currentTool = BuildPhaseTool.hand;
        currentEntity = BuildPhaseEntity.jammyWalker;
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
        currentEntity = (BuildPhaseEntity)bpe;
        Debug.Log("Current entity set to " + currentEntity);
    }
}
