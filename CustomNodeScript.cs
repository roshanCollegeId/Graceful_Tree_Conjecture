using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class CustomNodeScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform _newNode;
    private Transform _currentInDragTransform;
    private NodeInstance _previousNodeInstance;
    private bool _perfectPositioning = true;
    public static bool AllPerfect;

    private void Update()
    {
        if (!CustomMapCreation.GetTheInstance().completedNodeInstantiation) return;
        AllPerfect = PerfectionCheck();
        
        foreach (var inst in NodeClass.NodeObjectListNodeInstance)
        {
            if (FoundAnOverlappingCase(inst.NodeTransform))
            {
                GameAssets.GetTheInstance().infoTabText.text = $"Warning: Remove The Nodes Overlapping";
                GameAssets.GetTheInstance().infoDisplayTime = 2f;
                AllPerfect = false;
            }

            var boundary = GameAssets.GetTheInstance().gameBoundary.GetComponent<RectTransform>().rect;
            var yOffset = GameAssets.GetTheInstance().gameBoundary.transform.position.y;
            float radii = Dimensions.GetThePrefabDimension.Height * 0.5f;
            
            // Nodes not to go out of Boundaries
            if (inst.NodeTransform.position.x < boundary.width * 0.5f * -1f + radii||
                inst.NodeTransform.position.x > boundary.width * 0.5f - radii)
            {
                GameAssets.GetTheInstance().infoTabText.text = $"Warning: Nodes strictly should be within boundaries";
                GameAssets.GetTheInstance().infoDisplayTime = 0.1f;
                AllPerfect = false;
            }
            
            else if (inst.NodeTransform.position.y < yOffset + boundary.height * 0.5f * -1f + radii ||
                     inst.NodeTransform.position.y > yOffset + boundary.height * 0.5f - radii)
            {
                GameAssets.GetTheInstance().infoTabText.text = $"Warning: Nodes strictly should be within boundaries";
                GameAssets.GetTheInstance().infoDisplayTime = 0.1f;
                AllPerfect = false;
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _currentInDragTransform = eventData.pointerDrag.transform;
        if (!CustomMapCreation.GetTheInstance().completedNodeInstantiation)
        {
            // Create a new Node
            _newNode = NodeClass.InstantiateTheNode(_currentInDragTransform.position).transform;

            _previousNodeInstance = NodeClass.GetTheNodeInstance(eventData.pointerDrag.transform);

            if (_previousNodeInstance.OutFlow >= 2)
            {
                GameObject lineRendererPrefab = Instantiate(GameAssets.GetTheInstance().linePrefab);
                _previousNodeInstance.LineRendererPrefab = lineRendererPrefab;
                _previousNodeInstance.LineRendererPrefab.GetComponent<LineRenderer>().positionCount = 2;
                var position = _previousNodeInstance.NodeTransform.position;
                _previousNodeInstance.LineRendererPrefab.GetComponent<LineRenderer>()
                    .SetPosition(0, new Vector3(position.x, position.y, 150f));
            }
            
            else
            {
                _previousNodeInstance.LineRendererPrefab.GetComponent<LineRenderer>().positionCount++;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CustomMapCreation.GetTheInstance().completedNodeInstantiation)
        {
            // Move The Node
            _newNode.GetComponent<RectTransform>().anchoredPosition +=
                eventData.delta / GameAssets.GetTheInstance().canvas.scaleFactor;

            var nodeInstance = NodeClass.GetTheNodeInstance(_currentInDragTransform);
            LineRenderer lineRenderer = nodeInstance.LineRendererPrefab.GetComponent<LineRenderer>();
            var position = _newNode.position;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(position.x, position.y, 150f));
        }
        else
        {
            GetComponent<RectTransform>().anchoredPosition += eventData.delta / GameAssets.GetTheInstance().canvas.scaleFactor;
            foreach (KeyValuePair<LineRenderer, int> entry in NodeClass.GetTheNodeInstance(_currentInDragTransform).LineDetails)
            {
                var position = _currentInDragTransform.position;
                entry.Key.SetPosition(entry.Value, new Vector3(position.x, position.y, 150f));
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CustomMapCreation.GetTheInstance().completedNodeInstantiation)
        {
            var nodeInstance = NodeClass.GetTheNodeInstance(_currentInDragTransform);

            // Keep a check on condition
            if (FoundAnOverlappingCase(_newNode))
            {
                GameAssets.GetTheInstance().infoTabText.text = $"Warning: Overlapping Not Allowed";
                GameAssets.GetTheInstance().infoDisplayTime = 2f;
                _perfectPositioning = false;
            }

            if (!_perfectPositioning)
            {
                nodeInstance.LineRendererPrefab.GetComponent<LineRenderer>().positionCount--;
                _perfectPositioning = true;
                Destroy(_newNode.gameObject);
            }
            
            else
            {
                NodeInstance newNodeInstance = new NodeInstance
                {
                    NodeTransform = _newNode, OutFlow = 0, LineRendererPrefab = nodeInstance.LineRendererPrefab,
                    LineDetails = new Dictionary<LineRenderer, int>
                        {
                            [nodeInstance.LineRendererPrefab.GetComponent<LineRenderer>()] = nodeInstance
                                .LineRendererPrefab.GetComponent<LineRenderer>().positionCount - 1
                        }
                };
                
                CustomMapCreation.LineSegments.Add(new[]{_previousNodeInstance.NodeTransform, newNodeInstance.NodeTransform});

                nodeInstance.OutFlow++;
                newNodeInstance.OutFlow++;
                
                if (nodeInstance.OutFlow > 2)
                {
                    nodeInstance.LineDetails.Add(newNodeInstance.LineRendererPrefab.GetComponent<LineRenderer>(), 0);
                    nodeInstance.ObjectBranchDataList = new List<GameObject> { nodeInstance.NodeTransform.gameObject };
                }

                newNodeInstance.ObjectBranchDataList = nodeInstance.ObjectBranchDataList;
                newNodeInstance.ObjectBranchDataList.Add(_newNode.gameObject);
                NodeClass.NodeObjectListNodeInstance.Add(newNodeInstance);
            }

            // Completed all instantiations
            if (NodeClass.NodeObjectListNodeInstance.Count >= CustomMapCreation.GetTheInstance().numberOfNodes)
            {
                GameAssets.GetTheInstance().infoTabText.color = Color.black;
                GameAssets.GetTheInstance().infoTabText.text = "Modify the map accordingly and press the okay button to confirm the map";
                GameAssets.GetTheInstance().infoDisplayTime = 60;
                CustomMapCreation.GetTheInstance().completedNodeInstantiation = true;

                List<List<GameObject>> temporaryObjectData = new List<List<GameObject>>();
                foreach (var variableNodeInstance in NodeClass.NodeObjectListNodeInstance.Where(variableNodeInstance =>
                             variableNodeInstance.OutFlow == 1)) 
                { temporaryObjectData.Add(variableNodeInstance.ObjectBranchDataList); }

                NodeClass.ObjectBranchData = CustomMapCreation.ObjectTo2dArray(temporaryObjectData);
                for (int i = 0; i < temporaryObjectData.Count; i++) { NodeClass.ObjectBranchData[i] = temporaryObjectData[i].ToArray(); }

                GameAssets.GetTheInstance().exitButton.SetActive(false);
                GameAssets.GetTheInstance().fixTheNodeButton.SetActive(true);
            }
        }
    }

    private static bool FoundAnOverlappingCase(Transform newNode)
    {
        foreach (var element in NodeClass.NodeObjectListNodeInstance)
        {
            if (element.NodeTransform == newNode.transform) continue;
            if (Vector2.Distance(element.NodeTransform.position, newNode.transform.position) <
                Dimensions.GetThePrefabDimension.Height * 1.16)
            {
                return true;
            }
        }

        return false;
    }

    private static bool PerfectionCheck()
    {
        foreach (var inst in CustomMapCreation.LineSegments)
        {
            foreach (var tempInst in CustomMapCreation.LineSegments)
            {
                // Check for the Node Overlapping
                float radii = Dimensions.GetThePrefabDimension.Height * 1.16f * 0.5f;

                if (inst[0] != tempInst[0] && inst[0] != tempInst[1])
                {
                    if (LineScript.PerpendicularDistanceOverlap(tempInst[0].position, tempInst[1].position,
                            inst[0].position, radii))
                    {
                        GameAssets.GetTheInstance().infoTabText.text = $"Warning: Nodes Should Not Be Overlapping";
                        GameAssets.GetTheInstance().infoDisplayTime = 2f;
                        return false;
                    }
                }

                if (inst[1] != tempInst[0] && inst[1] != tempInst[1])
                {
                    if (LineScript.PerpendicularDistanceOverlap(tempInst[0].position, tempInst[1].position,
                            inst[1].position, radii))
                    {
                        
                        GameAssets.GetTheInstance().infoTabText.text = $"Warning: Nodes Should Not Be Overlapping";
                        GameAssets.GetTheInstance().infoDisplayTime = 2f;
                        return false;
                    }
                }

                // Check for the Intersections
                if (inst[1] != tempInst[0] && inst[1] != tempInst[1] && inst[0] != tempInst[0] &&
                    inst[0] != tempInst[1])
                {
                    if (LineScript.IntersectingLines(inst[0].position, inst[1].position,
                            tempInst[0].position, tempInst[1].position))
                    {
                        GameAssets.GetTheInstance().infoTabText.text = $"Warning: Lines Should Not Be Intersecting";
                        GameAssets.GetTheInstance().infoDisplayTime = 2f;
                        return false;
                    }
                }
            }
        }
        
        return true;
    }
}