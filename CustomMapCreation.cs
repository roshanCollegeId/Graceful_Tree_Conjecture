using System.Collections.Generic;
using UnityEngine;

public class CustomMapCreation : MonoBehaviour
{
    private static CustomMapCreation _instance;
    public bool completedNodeInstantiation;
    public static List<Transform[]> LineSegments;
    public int numberOfNodes;
    private static List<List<Elements>> _previewBranches;

    public static CustomMapCreation GetTheInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
        numberOfNodes = ButtonsScript.Level;
    }

    public void TargetMap()
    {
        Dimensions.SetThePrefabDimensions(GameAssets.GetTheInstance().camera.orthographicSize, Dimensions.NodeFactor(numberOfNodes));
        LineSegments = new List<Transform[]>();
    }
    
    public int CustomCreation()
    {
        GameAssets.GetTheInstance().imagePrefab.GetComponent<NodeScript>().enabled = false;
        GameObject lineRendererPrefab = Instantiate(GameAssets.GetTheInstance().linePrefab);

        var firstNodeObject = NodeClass.InstantiateTheNode(new Vector3(0, 0, 0));
        NodeInstance firstNode = new NodeInstance
        {
            NodeTransform = firstNodeObject.transform,
            OutFlow = 1,
            LineRendererPrefab = lineRendererPrefab,
            LineDetails = new Dictionary<LineRenderer, int> {[lineRendererPrefab.GetComponent<LineRenderer>()] = 0},
            ObjectBranchDataList = new List<GameObject> {firstNodeObject},
        };

        firstNode.LineRendererPrefab.GetComponent<LineRenderer>().positionCount = 1;
        var position = firstNode.NodeTransform.position;
        firstNode.LineRendererPrefab.GetComponent<LineRenderer>()
            .SetPosition(0, new Vector3(position.x, position.y, 150f));

        NodeClass.NodeObjectListNodeInstance = new List<NodeInstance> {firstNode};
        return numberOfNodes;
    }
    
    public static GameObject[][] ObjectTo2dArray(List<List<GameObject>> list)
    {
        var result = new GameObject[list.Count][];
        for (int i = 0; i < list.Count; i++)
        {
            result[i] = new GameObject[list[i].Count];
        }
        
        return result;
    }
}
