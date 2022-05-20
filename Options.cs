using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    [HideInInspector] public int timesPlayed;
    [HideInInspector] public int timesWon;
    [HideInInspector] public int levelAchieved;
    private static Options _instance;
    [SerializeField] private GameObject experienceBar;
    [SerializeField] private TextMeshProUGUI exp;
    [SerializeField] private TextMeshProUGUI level;

    public static Options GetTheInstance()
    {
        return _instance;
    }
    private void Awake()
    {
        _instance = this;
    }
    private void Start()
    {
        timesPlayed = PlayerPrefs.GetInt("TimesPlayed");
        timesWon = PlayerPrefs.GetInt("TimesWon");
        levelAchieved = PlayerPrefs.GetInt("Level");
        MenuManager.IsACustomGame = Convert.ToBoolean(PlayerPrefs.GetInt("CustomGame"));

        exp.text = Convert.ToString(timesPlayed);
        level.text = Convert.ToString(levelAchieved);
        experienceBar.GetComponent<Slider>().value = (float) timesWon / timesPlayed;
        
        GameObject toggle = GameObject.Find("Toggle");
        toggle.GetComponent<Toggle>().isOn = MenuManager.IsACustomGame;
    }
}
