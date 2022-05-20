using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Transform levelParent;
    [SerializeField] private GameObject levelObject;
    [SerializeField] private TextMeshProUGUI difficultyLevelText;
    [SerializeField] private Scrollbar scrollBar;
    private const int HighestLevel = 40;

    private void Start()
    {
        FindObjectOfType<AudioManager>().Play("MenuMusic");
        for (int i = 0; i < HighestLevel; i++)
        {
            Transform clone = Instantiate(levelObject, levelParent).transform;
            clone.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
        }
    }

    private void Update()
    {
        if (scrollBar.value < 0.23)
        {
            difficultyLevelText.text = $"Impossible";
        }
        
        else if (scrollBar.value >= 0.23 && scrollBar.value < 0.5)
        {
            difficultyLevelText.text = $"Hard";
        }
        
        else if (scrollBar.value >= 0.5 && scrollBar.value < 0.8)
        {
            difficultyLevelText.text = $"Medium";
        }
        
        else
        {
            difficultyLevelText.text = $"Easy";
        }
    }
}
