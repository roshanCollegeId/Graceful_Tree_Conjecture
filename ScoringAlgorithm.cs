using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoringAlgorithm : MonoBehaviour
{
    private static List<Elements> _nodeList;
    private static ScoringAlgorithm _instance;

    public static ScoringAlgorithm GetTheInstance()
    {
        return _instance;
    }
    private void Awake()
    {
        _instance = this;
    }

    private static void ProvideTheMap(List<Elements> map)
    {
        _nodeList = map;
    }

    public float ScoreCalculator(List<Elements> map, List<List<Elements>> branches, int numberOfBlanks, bool junction)
    {
        ProvideTheMap(map);
        float heightScore = HeightAndWidthScore();
        float linkageScore = LinkageLengthScore(branches, numberOfBlanks);
        float numberOfJunctions = NumberOfJunctions(branches);
        float jointScore = ProvideJointAngleScore(branches);
        float score = heightScore + linkageScore + jointScore;
        if (junction) score += numberOfJunctions;

        return score;
    }

    public static float IdealPosition(int i)
    {
        float top = RandomMapGeneration.ScreenBoundsMax.y * 0.7f;
        float bottom = RandomMapGeneration.ScreenBoundsMin.y * 0.85f;
        float right = RandomMapGeneration.ScreenBoundsMax.x * 0.9f;
        float left = RandomMapGeneration.ScreenBoundsMin.x * 0.9f;

        return i switch
        {
            1 => right,
            -1 => left,
            2 => top,
            -2 => bottom,
            _ => 0
        };
    }

    private static Elements ExtremePositionedElements(int i)
    {
        Elements topmostElement = null;
        Elements bottommostElement = null;
        Elements extremeRightElement = null;
        Elements extremeLeftElement = null;

        float magnitudeTop = -9999f;
        float magnitudeBottom = 9999f;
        float magnitudeRight = -9999f;
        float magnitudeLeft = 9999f;

        foreach (var nodes in _nodeList)
        {
            if (nodes.Position.y > magnitudeTop)
            {
                topmostElement = nodes;
                magnitudeTop = nodes.Position.y + Dimensions.GetThePrefabDimension.Height;
            }

            if (nodes.Position.y < magnitudeBottom)
            {
                bottommostElement = nodes;
                magnitudeBottom = nodes.Position.y - Dimensions.GetThePrefabDimension.Height * 0.5f;
            }

            if (nodes.Position.x > magnitudeRight)
            {
                extremeRightElement = nodes;
                magnitudeRight = nodes.Position.x + Dimensions.GetThePrefabDimension.Height * 0.5f;
            }

            if (!(nodes.Position.x < magnitudeLeft)) continue;
            extremeLeftElement = nodes;
            magnitudeLeft = nodes.Position.x - Dimensions.GetThePrefabDimension.Height * 0.5f;
        }

        return i switch
        {
            1 => extremeRightElement,
            -1 => extremeLeftElement,
            2 => topmostElement,
            -2 => bottommostElement,
            _ => null
        };
    }

    private static float ExaggerateTheScore(float score)
    {
        return Mathf.Sqrt(1 - score * score);
    }

    private static float HeightAndWidthScore()
    {
        float radii = Dimensions.GetThePrefabDimension.Height * 0.5f;
        bool rejectedCase = ExtremePositionedElements(1).Position.x < 0 ||
                            ExtremePositionedElements(-1).Position.x > 0 ||
                            ExtremePositionedElements(2).Position.y < 0 ||
                            ExtremePositionedElements(-2).Position.y > 0 ||
                            ExtremePositionedElements(1).Position.x > RandomMapGeneration.ScreenBoundsMax.x - radii ||
                            ExtremePositionedElements(-1).Position.x < RandomMapGeneration.ScreenBoundsMin.x + radii ||
                            ExtremePositionedElements(2).Position.y > RandomMapGeneration.ScreenBoundsMax.y - radii ||
                            ExtremePositionedElements(-2).Position.y < RandomMapGeneration.ScreenBoundsMin.y + radii;

        if (rejectedCase) return -100f;

        // If exceed then bigger problem
        float rightFactor = IdealPosition(1) - ExtremePositionedElements(1).Position.x;
        float leftFactor = IdealPosition(-1) - ExtremePositionedElements(-1).Position.x;
        float topFactor = IdealPosition(2) - ExtremePositionedElements(2).Position.y;
        float bottomFactor = IdealPosition(-2) - ExtremePositionedElements(-2).Position.y;

        float topScore = 0, bottomScore = 0, rightScore = 0, leftScore = 0;

        if (topFactor >= 0) topScore = (IdealPosition(2) - topFactor) / IdealPosition(2);
        if (topFactor < 0)
            topScore = (RandomMapGeneration.ScreenBoundsMax.y - ExtremePositionedElements(2).Position.y) /
                       (RandomMapGeneration.ScreenBoundsMax.y - IdealPosition(2));
        if (bottomFactor >= 0)
            bottomScore = (ExtremePositionedElements(-2).Position.y - RandomMapGeneration.ScreenBoundsMin.y) /
                          (IdealPosition(-2) - RandomMapGeneration.ScreenBoundsMin.y);
        if (bottomFactor < 0) bottomScore = (IdealPosition(-2) - bottomFactor) / IdealPosition(-2);
        if (rightFactor >= 0) rightScore = (IdealPosition(1) - rightFactor) / IdealPosition(1);
        if (rightFactor < 0)
            rightScore = (RandomMapGeneration.ScreenBoundsMax.x - ExtremePositionedElements(1).Position.x) /
                         (RandomMapGeneration.ScreenBoundsMax.x - IdealPosition(1));
        if (leftFactor >= 0)
            leftScore = (ExtremePositionedElements(-1).Position.x - RandomMapGeneration.ScreenBoundsMin.x) /
                        (IdealPosition(-1) - RandomMapGeneration.ScreenBoundsMin.x);
        if (leftFactor < 0) leftScore = (IdealPosition(-1) - leftFactor) / IdealPosition(-1);

        float totalHeightAndWidthScore = ExaggerateTheScore(topScore) + ExaggerateTheScore(bottomScore) + ExaggerateTheScore(rightScore) + ExaggerateTheScore(leftScore);

        return totalHeightAndWidthScore;
    }

    private static float LinkageLengthScore(List<List<Elements>> branches, int numberOfBlanks)
    {
        Vector2[,] mapInfo = LinkageAlgorithm.ProvideTheMapInfo(branches, numberOfBlanks);

        float[] linkLength = new float[numberOfBlanks - 1];
        float eX = 0;
        float eX2 = 0;
        for (int i = 0; i < mapInfo.GetLength(0); i++)
        {
            linkLength[i] = Vector3.Distance(mapInfo[i, 0], mapInfo[i, 1]);
            eX += linkLength[i];
            eX2 += linkLength[i] * linkLength[i];
        }

        eX /= linkLength.Length;
        eX2 /= linkLength.Length;

        float sigma = Mathf.Sqrt(eX2 - eX * eX) * 100f / RandomMapGeneration.OrthographicSize;

        return ExaggerateTheScore(sigma < 40f ? (40f - sigma) * 0.025f : 0f);
    }
    
    // Higher Angled Joints
    private static float ProvideJointAngleScore(List<List<Elements>> branches)
    {
        List<float> scores = new List<float>();
        foreach (var list in branches)
        {
            List<float> angleArray = new List<float>();
            List<Vector2> previousVector = new List<Vector2>();
            for (int i = 0; i < list.Count; i++)
            {
                previousVector.Add(list[i].Position);
                if (i > 0) angleArray.Add(1f - Vector2.Dot(previousVector[i-1].normalized, previousVector[i].normalized));
                if (angleArray == null) throw new ArgumentNullException(nameof(angleArray));
            }

            scores.Add(angleArray.Sum() * 0.5f);
        }
        
        return scores.Average();
    }

    private static int NumberOfJunctions(List<List<Elements>> branches)
    {
        foreach (var list in branches)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 0 || i == list.Count - 1) list[i].Junction = 2;
                else list[i].Junction = 1;
            }
        }

        foreach (var element in branches.SelectMany(list => list)) element.Junction -= 1; 

        return _nodeList.Where(element => element.Junction > 0).Sum(element => element.Junction);
    }
}