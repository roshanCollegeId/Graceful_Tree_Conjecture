using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Random;
using Random = System.Random;

public class RandomMapGeneration : MonoBehaviour
{
    private static RandomMapGeneration _instance;

    public static RandomMapGeneration GetTheInstance()
    {
        return _instance;
    }

    public static Vector3 ScreenBoundsMax;
    public static Vector3 ScreenBoundsMin;

    private int _numberOfBlanks;

    private const float RadiiFactor = 0.5f;
    private const float GameRegionBoundaryFactor = 0.7f;

    public static float OrthographicSize;
    private List<CollectionOfMaps> _collectionOfMapsList;

    private static bool _completedThread;
    
    private void Awake()
    {
        _instance = this;
        _numberOfBlanks = ButtonsScript.Level;
        OrthographicSize = GameAssets.GetTheInstance().camera.orthographicSize;
        GameAssets.GetTheInstance().camera.GetComponent<Physics2DRaycaster>().eventMask &= ~(1 << 8);
        GameAssets.GetTheInstance().canvas.GetComponent<GraphicRaycaster>().blockingMask = ~(1 << 8);

        ScreenBoundsMax = GameAssets.GetTheInstance().camera
            .ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0f)) * 0.85f;
        ScreenBoundsMin = GameAssets.GetTheInstance().camera.ScreenToWorldPoint(new Vector3(0, 0, 0f)) * 0.85f;
    }

    private void Update()
    {
        if (!_completedThread) return;
        if (GameAssets.GetTheInstance().isPermanentCustomCreation) return;

        if (CollectionOfMaps.FinalMap != null)
        {
            NodeClass.NodeObjectList = new List<Transform>();
            NodeClass.ObjectBranchData = ElementsTo2dArray(CollectionOfMaps.FinalMap.Map);
            foreach (var t in CollectionOfMaps.FinalMap.NodeList)
            {
                var node = t;
                var clone = NodeClass.InstantiateTheNode(new Vector3(node.Position.x, node.Position.y, 110f));
                NodeClass.NodeObjectList.Add(clone.transform);

                // Branch Data to Object Branch Data
                for (int x = 0; x < CollectionOfMaps.FinalMap.Map.Count; x++)
                {
                    List<Elements> list = CollectionOfMaps.FinalMap.Map[x];
                    int index = list.IndexOf(t);
                    if (index >= 0) NodeClass.ObjectBranchData[x][index] = clone;
                }
            }

            LinkageAlgorithm.ShowTheLinkage(CollectionOfMaps.FinalMap.Map);
            if (_collectionOfMapsList != null) GameManager.ClearMemoryList(_collectionOfMapsList);
        }

        _completedThread = false;
    }
    public int GenerateTheMap()
    {
        Dimensions.SetThePrefabDimensions(OrthographicSize, Dimensions.NodeFactor(_numberOfBlanks));
        GameAssets.GetTheInstance().imagePrefab.GetComponent<CustomNodeScript>().enabled = false;
        _collectionOfMapsList = new List<CollectionOfMaps>();
        Coin.SpawnTheCoins(_numberOfBlanks);
        GenerateRandomPoints(_numberOfBlanks, _collectionOfMapsList, RadiiFactor, ScreenBoundsMax, ScreenBoundsMin);

        return _numberOfBlanks;
    }

    private void GenerateRandomPoints(int numberOfBlanksGiven,
        List<CollectionOfMaps> collectionOfMapsList, float radiiFactor, Vector3 screenBoundMax, Vector3 screenBoundMin)
    {
        int numberOfIterations = (int)(100 * ((10f - (int)(numberOfBlanksGiven / 5f)) * 0.1f));
        float[] randomArray = new float[numberOfIterations * numberOfBlanksGiven + 1];

        var rand = MenuManager.PermanentTutorialSession ? new Random(12) : new Random(Range(1, 1000));

        for (int i = 0; i < randomArray.Length; i++)
        {
            randomArray[i] = (float)rand.NextDouble();
        }

        float currentScore = 0f;
        CollectionOfMaps.FinalMap = null;

        int randomIndex = 0;
        for (int samples = 0; samples < numberOfIterations; samples++)
        {
            List<Elements> blanksList = new List<Elements>();
            for (int i = 0; i < numberOfBlanksGiven; i++)
            {
                blanksList.Add(new Elements
                {
                    Position = SetTheRandomPosition(Dimensions.GetThePrefabDimension.Width * radiiFactor,
                        Dimensions.GetThePrefabDimension.Height * radiiFactor, screenBoundMax,
                        screenBoundMin, randomArray[randomIndex])
                });
                randomIndex++;
            }

            // Create a Native Array
            NativeArray<Vector2> blankListPositionOnlyNativeArray =
                new NativeArray<Vector2>(blanksList.Count, Allocator.TempJob);
            NativeArray<bool> processInput = new NativeArray<bool>(1, Allocator.TempJob);

            for (int i = 0; i < blanksList.Count; i++)
            {
                blankListPositionOnlyNativeArray[i] = blanksList[i].Position;
            }

            ChaoticJob job = new ChaoticJob
            {
                ScreenBoundsMin = ScreenBoundsMin,
                ScreenBoundsMax = ScreenBoundsMax,
                GameRegionBoundaryFactor = GameRegionBoundaryFactor,
                Diameter = Dimensions.GetThePrefabDimension.Height,
                Process = processInput,
                IdealPosition2 = ScoringAlgorithm.IdealPosition(2),
                BlankListPositionOnlyNativeArray = blankListPositionOnlyNativeArray
            };

            JobHandle jobHandle = job.Schedule();
            jobHandle.Complete();

            // Get Back The Data
            var process = processInput[0];
            for (int i = 0; i < blanksList.Count; i++)
            {
                blanksList[i].Position = blankListPositionOnlyNativeArray[i];
            }

            blankListPositionOnlyNativeArray.Dispose();
            processInput.Dispose();

            var branches = LinkageAlgorithm.GetTheInstance().Link(blanksList, false);

            if (process && CheckForValidCases(branches, blanksList))
            {
                CollectionOfMaps mapCollection = new CollectionOfMaps
                {
                    NodeList = blanksList, Map = branches,
                    Score = ScoringAlgorithm.GetTheInstance().ScoreCalculator(blanksList, branches, numberOfBlanksGiven, true)
                };

                collectionOfMapsList.Add(mapCollection);

                if (mapCollection.Score > currentScore)
                {
                    CollectionOfMaps.FinalMap = mapCollection;
                    currentScore = mapCollection.Score;
                }
            }

            if (samples == numberOfIterations - 1) { _completedThread = true; }
        }

        if (collectionOfMapsList == null) throw new ArgumentNullException(nameof(collectionOfMapsList));
    }
    private static Vector3 SetTheRandomPosition(float offsetX, float offsetY, Vector3 screenBoundsMaximum,
        Vector3 screenBoundsMinimum, float random)
    {
        float x = screenBoundsMinimum.x + offsetX +
                  (screenBoundsMaximum.x - offsetX - (screenBoundsMinimum.x + offsetX)) * FX(3, random);
        float y = screenBoundsMinimum.y + offsetY +
                  (screenBoundsMaximum.y - offsetY - (screenBoundsMinimum.y + offsetY)) * FX(11 / 3, random);
        float z = 0f * FX(0, random);
        Vector3 position = new Vector3(x, y, z);

        return position;
    }
    private static bool CheckForValidCases(IReadOnlyCollection<List<Elements>> branches, List<Elements> nodeList)
    {
        foreach (var list in branches)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                if (LineScript.Intersect(list[i], list[i + 1], branches) ||
                    LineScript.OverlapWithNodes(list[i], list[i + 1], nodeList, Dimensions.GetThePrefabDimension.Height * 1.16f * 0.5f))
                { return false; }
            }
        }

        return true;
    }
    private static float ExponentialDistributionFunction(float x, float k, float averageX, float averageY)
    {
        float a = 2 * (k - 1);
        float fx = a * Mathf.Pow(x - averageX, k) + averageY;
        return fx;
    }
    private static float FX(int orderOfEquation, float x)
    {
        const float averageX = RadiiFactor;
        const float averageY = RadiiFactor;
        float fx = ExponentialDistributionFunction(x, orderOfEquation, averageX, averageY);
        return fx;
    }

    private static GameObject[][] ElementsTo2dArray(List<List<Elements>> list)
    {
        var result = new GameObject[list.Count][];
        for (int i = 0; i < list.Count; i++)
        {
            result[i] = new GameObject[list[i].Count];
        }
        
        return result;
    }
}

public class Elements
{
    public Elements NextAttachedElement { get; set; }
    public int Junction { get; set; }
    [DefaultValue(false)] public bool Activated { get; set; }
    public Vector3 Position { get; set; }
}

public class CollectionOfMaps
{
    public List<Elements> NodeList { get; set; } 
    public List<List<Elements>> Map { get; set; }
    public float Score { get; set; }
    public static CollectionOfMaps FinalMap { get; set; }
}

public struct ChaoticJob : IJob
{
    public Vector2 ScreenBoundsMin;
    public Vector2 ScreenBoundsMax;
    public float GameRegionBoundaryFactor;
    public float Diameter;
    public NativeArray<bool> Process;
    public float IdealPosition2;
    public NativeArray<Vector2> BlankListPositionOnlyNativeArray;

    public void Execute()
    {
        PositionToCenterOfView(BlankListPositionOnlyNativeArray);
        Process[0] = RepositionTheNodes(BlankListPositionOnlyNativeArray, Diameter);
    }
    
    private int OutOfBounds(Vector3 element)
    {
        if (element.y < ScreenBoundsMin.y + Diameter * GameRegionBoundaryFactor)
            return -2;
        if (element.y > ScoringAlgorithm.IdealPosition(2)) return 2;
        if (element.x < ScreenBoundsMin.x + Diameter * GameRegionBoundaryFactor)
            return -1;

        return element.x > ScreenBoundsMax.x -
            Diameter * GameRegionBoundaryFactor
                ? 1
                : 0;
    }
    private void PositionToCenterOfView(NativeArray<Vector2> blankListPositionOnlyNativeArray) 
    {
        if (blankListPositionOnlyNativeArray.Length == 0) return;

        float averageX = 0f;
        float averageY = 0f;

        foreach (var node in blankListPositionOnlyNativeArray)
        {
            var position = node;
            averageX += position.x;
            averageY += position.y;
        }

        averageX /= blankListPositionOnlyNativeArray.Length;
        averageY /= blankListPositionOnlyNativeArray.Length;

        for (int i = 0; i < blankListPositionOnlyNativeArray.Length; i++)
        {
            var position = blankListPositionOnlyNativeArray[i];
            position = new Vector3(position.x - averageX, position.y - averageY);
            switch (OutOfBounds(position))
            {
                case 1:
                    position.x = 0f;
                    break;
                case -1:
                    position.x = 0f;
                    break;
                case 2:
                    position.y = 0f;
                    break;
                case -2:
                    position.y = 0f;
                    break;
            }

            blankListPositionOnlyNativeArray[i] = position;
        }
    }
    private bool RepositionTheNodes(NativeArray<Vector2> blankListPositionOnlyNativeArray, float diameterInput)
    {
        int depth = 0;
        int iterate = 0;
        while (iterate < blankListPositionOnlyNativeArray.Length)
        {
            int repeat = 0;
            while (true)
            {
                NativeArray<int> setOfOverlaps = CollisionDetectionMethod(blankListPositionOnlyNativeArray[iterate],
                    iterate, blankListPositionOnlyNativeArray, diameterInput);
                var size = setOfOverlaps.Length;

                repeat += size;
                if (size > 0)
                {
                    Vector2 other = blankListPositionOnlyNativeArray[setOfOverlaps[0]];
                    var position = blankListPositionOnlyNativeArray[iterate];
                    Vector2 directionVector = position - other;
                    var magnitude = Diameter - Vector3.Magnitude(directionVector);
                    directionVector += directionVector.normalized * magnitude;
                    position += directionVector;

                    if (OutOfBounds(position) != 0)
                    {
                        float x0, y0, x1, y1;
                        switch (Mathf.Abs(OutOfBounds(position)))
                        {
                            case 1:
                                x0 = blankListPositionOnlyNativeArray[iterate].x;
                                y0 = blankListPositionOnlyNativeArray[iterate].y;
                                x1 = position.x;
                                y1 = position.y;

                                float x = OutOfBounds(position) > 0
                                    ? ScreenBoundsMax.x - Diameter *
                                    GameRegionBoundaryFactor
                                    : ScreenBoundsMin.x + Diameter *
                                    GameRegionBoundaryFactor;

                                float y = (y1 - y0) / (x1 - x0) * x - (y0 * x1 - y1 * x0) / (x1 - x0);

                                Vector2 partialVector = new Vector2(x, y) - blankListPositionOnlyNativeArray[iterate];
                                Vector2 offsetVector = position - new Vector2(x, y);
                                Vector2 flippedDestinationVector =
                                    new Vector2(offsetVector.x * -1f, offsetVector.y);
                                position = partialVector + flippedDestinationVector;
                                break;

                            case 2:
                                x0 = blankListPositionOnlyNativeArray[iterate].x;
                                y0 = blankListPositionOnlyNativeArray[iterate].y;
                                x1 = position.x;
                                y1 = position.y;

                                y = OutOfBounds(position) > 0
                                    ? IdealPosition2
                                    : ScreenBoundsMin.y + Diameter *
                                    GameRegionBoundaryFactor;

                                x = (y + (y0 * x1 - y1 * x0) / (x1 - x0)) * ((x1 - x0) / (y1 - y0));

                                partialVector = new Vector2(x, y) - blankListPositionOnlyNativeArray[iterate];
                                offsetVector = position - new Vector2(x, y);
                                flippedDestinationVector =
                                    new Vector2(offsetVector.x, offsetVector.y * -1f);
                                position = partialVector + flippedDestinationVector;
                                break;
                        }
                    }
                    
                    blankListPositionOnlyNativeArray[iterate] = position;
                    if (depth > 50000) return false;
                    depth++;
                }
                else { break; }

                setOfOverlaps.Dispose();
            }

            iterate++;
            if (repeat > 0)
            {
                iterate = 0;
            }
        }

        return true;
    }
    private static NativeList<int> CollisionDetectionMethod(Vector2 node, int index, NativeArray<Vector2> blankListCopy, float diameterInput)
    {
        NativeList<int> result = new NativeList<int>(Allocator.Temp);
        float diameter = diameterInput * 1.16f;

        for (int i = 0; i < blankListCopy.Length; i++)
        {
            if (i == index) continue;
            if (Vector2.Distance(blankListCopy[i], node) < diameter) result.Add(i);
        }

        return result;
    }
}
