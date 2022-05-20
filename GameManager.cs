using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

/* Ideas
    1. Alchemy Version
    2. Stars and Constellation Sky Version
    3. Color Mixing Version */

// TODO: Modify the tutorial section

public class GameManager : MonoBehaviour
{
    private int _updateTimer = 1;
    private static int _numberOfNodes;
    private bool _completedPreviewCreation;
    public static bool Confirmed;
    public static List<GameObject> AllObjects;

    private void Start()
    {
        FindObjectOfType<AudioManager>().Stop("MenuMusic");
        FindObjectOfType<AudioManager>().Play(MenuManager.Music);
        Options.GetTheInstance().timesPlayed++;
        PlayerPrefs.SetInt("TimesPlayed", Options.GetTheInstance().timesPlayed);
        
        NodeClass.NodeObjectList = new List<Transform>();
        AllObjects = new List<GameObject>();
        Confirmed = false;
        
        if (!MenuManager.IsACustomGame)
        {
            if (!MenuManager.TutorialSession) _numberOfNodes = RandomMapGeneration.GetTheInstance().GenerateTheMap();
            else
            {
                GameAssets.GetTheInstance().tutorialWindow.SetActive(true);
                GameAssets.GetTheInstance().tutorialWindow.GetComponent<TextMeshProUGUI>().fontSize = 17;
                GameAssets.GetTheInstance().tutorialWindow.GetComponent<TextMeshProUGUI>().text =
                    $"Fill all the nodes with given " + 
                    $"consecutive odd numbers in a manner so that " +
                    $"any two directly connected nodes will have a unique " +
                    "absolute difference value";
            }
        }

        else
        {
            CustomMapCreation.GetTheInstance().TargetMap();

            if (MenuManager.TutorialSession)
            {
                GameAssets.GetTheInstance().tutorialWindow.SetActive(true);
                GameAssets.GetTheInstance().tutorialWindow.GetComponent<TextMeshProUGUI>().fontSize = 25;
                GameAssets.GetTheInstance().tutorialWindow.GetComponent<TextMeshProUGUI>().text = $"Make a map of {ButtonsScript.Level} nodes by drag and drop that you will have to solve";
            }
            
            _completedPreviewCreation = true;
        }
    }

    private void Update()
    {
        if (MenuManager.IsACustomGame && _completedPreviewCreation && !Confirmed && (!MenuManager.TutorialSession || MenuManager.TutorialSession && Input.GetMouseButtonDown(0)) )
        {
            // TODO: Work on nest phase after touch input
            FindObjectOfType<AudioManager>().Play("ButtonClick");
            GameAssets.GetTheInstance().tutorialWindow.SetActive(false);
            _numberOfNodes = CustomMapCreation.GetTheInstance().CustomCreation();
            MenuManager.IsACustomGame = true;
            _completedPreviewCreation = false;
        }

        if (MenuManager.TutorialSession && !MenuManager.IsACustomGame && Input.GetMouseButtonDown(0))
        {
            FindObjectOfType<AudioManager>().Play("ButtonClick");
            GameAssets.GetTheInstance().tutorialWindow.SetActive(false);
            _numberOfNodes = RandomMapGeneration.GetTheInstance().GenerateTheMap();
            MenuManager.TutorialSession = false;
        }
        
        if (Time.time > _updateTimer && !Confirmed)
        {
            _updateTimer = Mathf.FloorToInt(Time.time) + 1;
            Updater();
        }
    }

    private static void Updater()
    {
        if (Confirmed) { return; }
        
        if (NodeClass.NodeObjectList == null || NodeClass.NodeObjectList.Count == 0) return;

        if (NodeClass.NodeObjectList.Any(node => node.GetComponent<NodeScript>().AttachedCoinNumbering == 0))
        { return; }
        
        var textObject = GameAssets.GetTheInstance().infoTabText;
        textObject.color = Color.black;
        textObject.text = $"Press the confirm button to confirm you solution";
        GameAssets.GetTheInstance().infoDisplayTime = 60;
        
        GameAssets.GetTheInstance().exitButton.SetActive(false);
        GameAssets.GetTheInstance().fixTheNodeButton.SetActive(false);
        GameAssets.GetTheInstance().confirmButton.SetActive(true);
        Confirmed = true;
    }
    
    public void ConfirmTheSolution()
    {
        // Clear the Screen
        if (AllObjects.Count == 0)
        {
            // Nodes
            var o = GameObject.Find($"Node(Clone)");
            while (o)
            {
                o.name = "Node(Clone)" + "-D";
                AllObjects.Add(o);
                o.SetActive(false);
                o = GameObject.Find("Node(Clone)");
            }

            // Coins
            o = GameObject.Find($"Coin(Clone)");
            while (o)
            {
                o.name = "Coin(Clone)" + "-D";
                AllObjects.Add(o);
                o.SetActive(false);
                o = GameObject.Find("Coin(Clone)");
            }

            // Lines
            o = GameObject.Find($"Line(Clone)");
            while (o)
            {
                o.name = "Line(Clone)" + "-D";
                AllObjects.Add(o);
                o.SetActive(false);
                o = GameObject.Find("Line(Clone)");
            }
        }

        else foreach (var obj in AllObjects) { obj.SetActive(false); }

        int[] occuredNumber = new int[_numberOfNodes - 1];
        int index = 0;

        for (int row = 0; row < NodeClass.ObjectBranchData.Length; row++)
        {
            var list = NodeClass.ObjectBranchData;
            for (int column = 0; column < list[row].Length - 1; column++)
            {
                int difference = list[row][column].GetComponent<NodeScript>().AttachedCoinNumbering -
                                 list[row][column + 1].GetComponent<NodeScript>().AttachedCoinNumbering;
                
                occuredNumber[index] = Mathf.Abs(difference);
                index++;
            }
        }
        
        bool solved = occuredNumber.Distinct().Count() == occuredNumber.Length;

        GameAssets.GetTheInstance().infoTab.SetActive(false);
        GameAssets.GetTheInstance().confirmButton.SetActive(false);
        GameAssets.GetTheInstance().exitButton.SetActive(true);

        FindObjectOfType<AudioManager>().Stop(MenuManager.Music);
        if (solved)
        {
            FindObjectOfType<AudioManager>().Play("GameWin");
            GameAssets.GetTheInstance().winScreen.SetActive(true);
            if (ButtonsScript.Level > Options.GetTheInstance().levelAchieved)
            { Options.GetTheInstance().levelAchieved = ButtonsScript.Level; PlayerPrefs.SetInt("Level", Options.GetTheInstance().levelAchieved);}
            
            Options.GetTheInstance().timesWon++;
            PlayerPrefs.SetInt("TimesWon", Options.GetTheInstance().timesWon);
            return;
        }
        
        FindObjectOfType<AudioManager>().Play("GameLose");
        GameAssets.GetTheInstance().loseScreen.SetActive(true);
        GameAssets.GetTheInstance().retryButton.SetActive(true);
        GameAssets.GetTheInstance().infoTab.SetActive(false);
        
        string text = $"Repetition\n";
        Dictionary<int, int> freqMap = occuredNumber.GroupBy(x => x)
            .Where(g => g.Count() > 1)
            .ToDictionary(x => x.Key, x => x.Count());

        foreach (var entry in freqMap)
        {
            text += $"{entry.Key}: {entry.Value} times\n";
        }

        GameAssets.GetTheInstance().mistakesText.text = text;
    }

    public static void ClearMemoryList<T>(List<T> list)
    {
        int identification = GC.GetGeneration(list);
        list.Clear();
        GC.Collect(identification, GCCollectionMode.Forced);
    }
}

public static class Dimensions
{
    public static void SetThePrefabDimensions(float orthographicSize, float numberOfNodesFactor)
    {
        GameObject lineGameObject = GameAssets.GetTheInstance().linePrefab.gameObject;

        float orthographicSizeFactor = orthographicSize / 100f;

        var lineRenderer = lineGameObject.GetComponent<LineRenderer>();
           
        NodeClass.NodeImageScale = Vector3.one * numberOfNodesFactor * 4f;
        lineRenderer.startWidth = lineRenderer.endWidth = 5f * numberOfNodesFactor * orthographicSizeFactor;

        Vector2 spriteSize = GameAssets.GetTheInstance().imagePrefab.GetComponent<SpriteRenderer>().sprite.rect.size;
        Vector2 localSpriteSize = spriteSize / GameAssets.GetTheInstance().imagePrefab.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit;
        Vector3 worldSize = localSpriteSize * orthographicSizeFactor * GameAssets.GetTheInstance().imagePrefab.transform.localScale.x / 3.5f;

        GetThePrefabDimension.Width = worldSize.x;
        GetThePrefabDimension.Height = worldSize.y;
    }

    public static float NodeFactor(int numberOfBlanks)
    {
        float value = (15f - (int) (numberOfBlanks * 0.2f)) / 15f;
        return value;
    }
    public static class GetThePrefabDimension
    {
        public static float Width
        { get; set; }
        public static float Height
        { get; set; }
    }
}

public static class Coin
{
    public static void ActivateOriginalCoinScale(Transform coinGameObjectTransform)
    {
        coinGameObjectTransform.localScale = Vector3.one * 3f;
    }
    public static void ActivateModifiedCoinScale(Transform coinGameObjectTransform, Transform nodeGameObjectTransform)
    {
        coinGameObjectTransform.localScale = nodeGameObjectTransform.localScale;
    }
    
    public static void SpawnTheCoins(int numberOfBlanks)
    {
        int numberOfCoins = numberOfBlanks;
        GameObject coinPrefab = GameAssets.GetTheInstance().coinPrefab;

        for (int i = 0; i < numberOfCoins; i++)
        {
            Transform clone = Object.Instantiate(coinPrefab, new Vector3(0, 0, 0), Quaternion.identity).transform;
            clone.SetParent(GameAssets.GetTheInstance().parentCoinScrollGameObject.transform);
            clone.SetSiblingIndex(GameAssets.GetTheInstance().parentCoinScrollGameObject.transform.childCount);
            ActivateOriginalCoinScale(clone);
            clone.GetComponent<NumberingScript>().Numbering = 2 * i + 1;
        }
    }
}

public class NodeInstance
{
    public Transform NodeTransform { get; set; }
    public int OutFlow { get; set; }
    public GameObject LineRendererPrefab { get; set; }
    public IDictionary<LineRenderer, int> LineDetails { get; set; }
    public List<GameObject> ObjectBranchDataList { get; set; }
}

public static class NodeClass
{
    public static GameObject[][] ObjectBranchData;
    public static List<Transform> NodeObjectList;
    public static List<NodeInstance> NodeObjectListNodeInstance;
    public static Vector3 NodeImageScale
    {
        set => GameAssets.GetTheInstance().imagePrefab.transform.localScale = value;
    }

    public static GameObject InstantiateTheNode(Vector3 position)
    {
        GameObject clone = Object.Instantiate(GameAssets.GetTheInstance().imagePrefab, position, Quaternion.identity,
            GameAssets.GetTheInstance().nodeParentGameObject.transform);
        
        Object.Instantiate(GameAssets.GetTheInstance().backImagePrefab, position, Quaternion.identity, clone.transform);
        
        return clone;
    }
    
    public static NodeInstance GetTheNodeInstance(Transform transform)
    {
        foreach (var nodeInstance in NodeObjectListNodeInstance)
        {
            if (nodeInstance.NodeTransform == transform)
            {
                return nodeInstance;
            }
        }

        return NodeObjectListNodeInstance[0];
    }
}