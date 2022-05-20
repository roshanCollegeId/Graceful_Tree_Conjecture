using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject musicIcon;
    [SerializeField] private GameObject soundIcon;
    [SerializeField] private Sprite soundSwitchedOn;
    [SerializeField] private Sprite soundSwitchedOff;
    [SerializeField] private Animator mainMenuAnimator;
    [SerializeField] private Animator optionsMenuAnimator;
    [SerializeField] private AudioMixer audioMix;
    [SerializeField] private Animator transition;
    [SerializeField] private TMP_Dropdown dropDownMusic;
    [SerializeField] private TMP_Dropdown dropDownGraphics;

    private static readonly int ShowMain = Animator.StringToHash("ShowMain");
    private static readonly int ShowOptions = Animator.StringToHash("ShowOptions");
    public static bool TutorialSession;
    public static bool PermanentTutorialSession;
    public static bool IsACustomGame;
    public static string Music = "AlmostBliss";

    private void Awake()
    {
        if (!PlayerPrefs.HasKey("MusicToggle")) PlayerPrefs.SetInt("MusicToggle", 1);
        if (!PlayerPrefs.HasKey("SoundToggle")) PlayerPrefs.SetInt("SoundToggle", 1);
        if (!PlayerPrefs.HasKey("TimesPlayed")) PlayerPrefs.SetInt("TimesPlayed", 0);
        if (!PlayerPrefs.HasKey("TimesWon")) PlayerPrefs.SetInt("TimesWon", 0); 
        if (!PlayerPrefs.HasKey("Level")) PlayerPrefs.SetInt("Level", 0);
        if (!PlayerPrefs.HasKey("CustomGame")) PlayerPrefs.SetInt("CustomGame", 0);
        if (!PlayerPrefs.HasKey("DropDownMusic")) PlayerPrefs.SetInt("DropDownMusic", 0);
        
        musicIcon.transform.GetComponent<SpriteRenderer>().sprite = PlayerPrefs.GetInt("MusicToggle") == 1 ? soundSwitchedOn : soundSwitchedOff;
        soundIcon.transform.GetComponent<SpriteRenderer>().sprite = PlayerPrefs.GetInt("SoundToggle") == 1 ? soundSwitchedOn : soundSwitchedOff;
        dropDownMusic.value = PlayerPrefs.GetInt("DropDownMusic");
        dropDownGraphics.value = QualitySettings.GetQualityLevel();
    }

    private void Start()
    {
        FindObjectOfType<AudioManager>().Play("MenuMusic");
        mainMenuAnimator.SetBool(ShowMain, true);
        optionsMenuAnimator.SetBool(ShowOptions, false);
    }

    public void PlayButtonFunction()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        UnityEngine.AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(1);
    }

    public void TutorialsButtonFunction()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        TutorialSession = true;
        PermanentTutorialSession = true;
        ButtonsScript.Level = 7;
        UnityEngine.AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(2);
    }

    public void OptionsButtonFunction()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        mainMenuAnimator.SetBool(ShowMain, false);
        optionsMenuAnimator.SetBool(ShowOptions, true);
    }

    public void AboutButtonFunction()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        SceneManager.LoadScene(3);
    }

    public void QuitButtonFunction()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        Application.Quit();
    }

    public void FromOptionsBackToMenu()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        FindObjectOfType<AudioManager>().Stop(Music);
        mainMenuAnimator.SetBool(ShowMain, true);
        optionsMenuAnimator.SetBool(ShowOptions, false);
    }
    
    public void MusicButton()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        if (PlayerPrefs.GetInt("MusicToggle") == 0)
        {
            PlayerPrefs.SetInt("MusicToggle", 1);
            musicIcon.transform.GetComponent<SpriteRenderer>().sprite = soundSwitchedOn;
            audioMix.SetFloat("MenuMusicVolume", 0);
        }

        else
        {
            PlayerPrefs.SetInt("MusicToggle", 0);
            musicIcon.transform.GetComponent<SpriteRenderer>().sprite = soundSwitchedOff;
            audioMix.SetFloat("MenuMusicVolume", -80);
        }
    }

    public void SoundButton()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        if (PlayerPrefs.GetInt("SoundToggle") == 0)
        {
            PlayerPrefs.SetInt("SoundToggle", 1);
            soundIcon.transform.GetComponent<SpriteRenderer>().sprite = soundSwitchedOn;
            audioMix.SetFloat("GameplayVolume", 0);
        }

        else
        {
            PlayerPrefs.SetInt("SoundToggle", 0);
            soundIcon.transform.GetComponent<SpriteRenderer>().sprite = soundSwitchedOff;
            audioMix.SetFloat("GameplayVolume", -80);
        }
    }

    public void SetDisplayQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}
