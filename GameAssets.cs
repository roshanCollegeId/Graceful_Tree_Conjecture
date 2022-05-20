using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public Canvas canvas;
    public new Camera camera;
    public GameObject nodeParentGameObject;
    public GameObject parentCoinScrollGameObject;
    public GameObject coinsParent;

    private static GameAssets _instance;
    public GameObject imagePrefab;
    public GameObject backImagePrefab;
    public GameObject coinPrefab;
    public GameObject linePrefab;
    public GameObject gameBoundary;
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject infoTab;
    public TextMeshProUGUI infoTabText;
    public TextMeshProUGUI mistakesText;
    [HideInInspector] public float infoDisplayTime;
    public GameObject tutorialWindow;
    public GameObject fixTheNodeButton;
    public GameObject exitButton;
    public GameObject confirmButton;
    public GameObject retryButton;

    [HideInInspector] public bool isPermanentCustomCreation;

    public static GameAssets GetTheInstance()
    {
        return _instance;
    }

    private void Awake()
    {
        _instance = this;
    }

    private void Start()
    {
        isPermanentCustomCreation = MenuManager.IsACustomGame;
        infoTab.SetActive(true);
        exitButton.SetActive(true);
        retryButton.SetActive(false);
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        fixTheNodeButton.SetActive(false);
        confirmButton.SetActive(false);
        tutorialWindow.SetActive(false);
        imagePrefab.GetComponent<NodeScript>().enabled = true;
        imagePrefab.GetComponent<CustomNodeScript>().enabled = true;
    }

    private void Update()
    {
        if (infoDisplayTime < 0f) { infoTabText.text = $""; }
        else { infoDisplayTime -= Time.deltaTime; }
        
        if (!MenuManager.TutorialSession) return;
        if (NodeClass.NodeObjectList == null || NodeClass.NodeObjectList.Any(node => node == null)) return;
        if (NodeClass.NodeObjectList.Any(node => node.GetComponent<NodeScript>().AttachedCoinNumbering != 0)) return;
        if (NodeClass.NodeObjectList.Count <= 0 || !Input.GetMouseButtonDown(0)) return;
        if (!tutorialWindow.activeSelf) return;
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        tutorialWindow.SetActive(false);

        foreach (var inst in NodeClass.NodeObjectListNodeInstance)
        {
            inst.NodeTransform.gameObject.SetActive(true);
            inst.LineRendererPrefab.SetActive(true);
        }
    }

    public void FixTheNodes()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        if (CustomNodeScript.AllPerfect)
        {
            NodeClass.NodeObjectList = new List<Transform>();
            foreach (var nodeInstance in NodeClass.NodeObjectListNodeInstance)
            {
                NodeClass.NodeObjectList.Add(nodeInstance.NodeTransform);
                nodeInstance.NodeTransform.GetComponent<CustomNodeScript>().enabled = false;
                nodeInstance.NodeTransform.GetComponent<NodeScript>().enabled = true;
            }

            // MenuManager.isACustomGame = false;
            exitButton.SetActive(true);
            fixTheNodeButton.SetActive(false);

            if (MenuManager.TutorialSession)
            {
                foreach (var nodeInstance in NodeClass.NodeObjectListNodeInstance)
                {
                    nodeInstance.NodeTransform.gameObject.SetActive(false);
                    nodeInstance.LineRendererPrefab.SetActive(false);
                }

                tutorialWindow.SetActive(true);
                tutorialWindow.GetComponent<TextMeshProUGUI>().fontSize = 17;
                tutorialWindow.GetComponent<TextMeshProUGUI>().text = $"Now fill all the nodes with given " +
                                                                      $"consecutive odd numbers in a manner so that " +
                                                                      $"any two directly connected nodes will have a unique " +
                                                                      "absolute difference value";
            }
            
            Coin.SpawnTheCoins(CustomMapCreation.GetTheInstance().numberOfNodes);
        }
    }
}
