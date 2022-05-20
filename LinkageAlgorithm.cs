using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LinkageAlgorithm : MonoBehaviour
{
    // ReSharper disable Unity.PerformanceAnalysis
    private static LinkageAlgorithm _instance;

    public static LinkageAlgorithm GetTheInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    public List<List<Elements>> Link(List<Elements> blanksList, bool show)
    {
        List<Elements> blanksListCopy = new List<Elements>(blanksList);
        List<List<Elements>> branches = new List<List<Elements>>();
        List<Elements> branchingData = new List<Elements>();
        int depth = 0;
        
        while (true)
        {
            branchingData.Clear();
            Elements startingGameObject = null;

            if (branches.Count != 0)
            {
                float magnitude = 9999f;
                foreach (var activatedElements in ActivatedOrInactivatedElementsList(blanksList)[0])
                {
                    foreach (var inactivatedElements in ActivatedOrInactivatedElementsList(blanksList)[1]
                        .Where(inactivatedElements =>
                            Vector3.Distance(activatedElements.Position, inactivatedElements.Position) < magnitude))
                    {
                        startingGameObject = activatedElements;
                        magnitude = Vector3.Distance(activatedElements.Position, inactivatedElements.Position);
                    }
                }
                
                if (startingGameObject != null)
                {
                    startingGameObject.Junction += 1;
                }
            }

            else
            {
                float magnitudeY = blanksListCopy[0].Position.y;
                foreach (var element in blanksListCopy.Where(element => element.Position.y > magnitudeY))
                {
                    startingGameObject = element;
                }
            }
            
            startingGameObject ??= blanksListCopy[0];
            
            branchingData.Add(startingGameObject);
            startingGameObject.Activated = true;
            blanksListCopy.Remove(startingGameObject);
            Elements nextElement = null;
            startingGameObject.NextAttachedElement = null;
            
            while (true)
            {
                if (branchingData.Count == blanksList.Count) break;
                int i = 0;
                var distance = 9999f;
                
                while (i < blanksListCopy.Count)
                {
                    if (blanksListCopy[i] == startingGameObject) { i++; continue; }
                    if (blanksListCopy[i].Activated) { i++; continue; }
                    if (Magnitude(startingGameObject, blanksListCopy[i]) < distance) nextElement = blanksListCopy[i];
                    if (nextElement != null) distance = Magnitude(startingGameObject, nextElement);
                    i++;
                }
                
                if (blanksList.Count == 0) break;
                if (nextElement == null) break;
                if (branchingData.Count >= 3 && Magnitude(startingGameObject, nextElement) >
                    Magnitude(startingGameObject, branchingData[branchingData.Count - 3])) break;
                if (LineScript.Intersect(startingGameObject, nextElement, branches)) break;

                branchingData.Add(nextElement);
                blanksListCopy.Remove(nextElement);
                nextElement.Activated = true;
                startingGameObject.NextAttachedElement = nextElement;
                startingGameObject = nextElement;
                nextElement = null;
            }
            
            List<Elements> branchingDataCopy = new List<Elements>(branchingData);
            branches.Add(branchingDataCopy);
            depth++;
            if (blanksListCopy.Count <= 0 || depth > 100) break;
        }
        if (show) ShowTheLinkage(branches);
        return branches;
    }
    private static float Magnitude(Elements gameObject1, Elements gameObject2)
    {
        return Vector3.Magnitude(gameObject1.Position - gameObject2.Position);
    }
    public static void ShowTheLinkage(List<List<Elements>> branches)
    {
        GameObject lineParent = GameObject.FindGameObjectWithTag("LineParent");
        foreach (var list in branches)
        {
            GameObject lineRendererPrefab = Instantiate(GameAssets.GetTheInstance().linePrefab, Vector3.one, Quaternion.identity, lineParent.transform);
            LineRenderer lineRenderer = lineRendererPrefab.GetComponent<LineRenderer>();

            lineRenderer.positionCount = 0;

            for (int i = 0; i < list.Count; i++)
            {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(i, new Vector3(list[i].Position.x, list[i].Position.y, 150f));
            }
            //Destroy(lineRendererPrefab, GameManager.GetTheInstance()._timeInterval);
        }
    }
    public static Vector2[,] ProvideTheMapInfo(List<List<Elements>> branches, int numberOfBlanks)
    {
        Vector2[] positionArray = new Vector2[numberOfBlanks];

        foreach (var list in branches)
        {
            for (int i = 0; i < list.Count; i++)
            {
                positionArray[i] = list[i].Position;
            }
        }
        
        Vector2[,] result = new Vector2[positionArray.Length - 1, 2];
        for (int i = 0; i < positionArray.Length - 1; i++)
        {
            result[i, 0] = positionArray[i];
            result[i, 1] = positionArray[i + 1];
        }
        
        return result;
    }
    private static List<Elements>[] ActivatedOrInactivatedElementsList(List<Elements> blankList)
    {
        List<Elements>[] result = new List<Elements>[2];
        List<Elements> activatedList = new List<Elements>();
        List<Elements> inactivatedList = new List<Elements>();
        foreach (var element in blankList)
        {
            if (element.Activated) activatedList.Add(element);
            else inactivatedList.Add(element);
        }

        result[0] = activatedList;
        result[1] = inactivatedList;
        
        return result;
    }
}
