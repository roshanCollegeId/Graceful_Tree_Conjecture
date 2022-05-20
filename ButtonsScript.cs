using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using AsyncOperation = UnityEngine.AsyncOperation;

public class ButtonsScript : MonoBehaviour
{
    public static int Level;
    [SerializeField] private AudioMixer audioMixer;

    public void IsToggleCustomGame()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        GameObject toggle = GameObject.Find($"Toggle");
        MenuManager.IsACustomGame = toggle.GetComponent<Toggle>().isOn;
        PlayerPrefs.SetInt("CustomGame", Convert.ToInt16(MenuManager.IsACustomGame));
    }
    public void ExitToLevels()
    {
        var audioManager = FindObjectOfType<AudioManager>();
        audioManager.Stop(MenuManager.Music);
        audioManager.Stop("GameWin");
        audioManager.Stop("GameLose");
        audioManager.Play("ButtonClick");
        foreach (var obj in GameManager.AllObjects) Destroy(obj);
        MenuManager.IsACustomGame = Convert.ToBoolean(PlayerPrefs.GetInt("CustomGame"));

        Debug.Log("Yes this one !!!");
        SceneManager.LoadSceneAsync(MenuManager.PermanentTutorialSession ? 0 : 1);
        MenuManager.PermanentTutorialSession = false;
    }
    
    public void RetryTheCurrentGame()
    {
        var audioManager = FindObjectOfType<AudioManager>();
        audioManager.Stop("GameWin");
        audioManager.Stop("GameLose");
        audioManager.Play("ButtonClick");
        audioManager.Play(MenuManager.Music);
        
        // Turn off all the scenes
        GameAssets.GetTheInstance().winScreen.SetActive(false);
        GameAssets.GetTheInstance().loseScreen.SetActive(false);
        GameAssets.GetTheInstance().retryButton.SetActive(false);
        GameAssets.GetTheInstance().infoTab.SetActive(true);
        GameManager.Confirmed = false;

        if (GameManager.AllObjects == null) return;
        foreach (var obj in GameManager.AllObjects) obj.SetActive(true);
    }
    public void EnterTheLevel()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        GameObject buttonObject = EventSystem.current.currentSelectedGameObject;
        Level = Convert.ToInt16(buttonObject.transform.parent.GetChild(0).GetComponent<TextMeshProUGUI>().text) + 2;

        SceneManager.LoadSceneAsync(sceneBuildIndex: 4);
        AsyncOperation loadingOperation = SceneManager.LoadSceneAsync(2);
    }

    public void BackToMainMenu()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        SceneManager.LoadScene(0);
    }
    
    private static IEnumerator LoadTheMenuScene(Animator transition)
    {
        transition.SetTrigger($"LoadLevel");

        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(0);
    }

    public void OpenYtVideo()
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        Application.OpenURL($"https://www.youtube.com/watch?v=v5KWzOOhZrw&t=170s");
    }

    public void HandleInputData(int val)
    {
        FindObjectOfType<AudioManager>().Play("ButtonClick");
        
        switch (val)
        {
            case 0:
                MenuManager.Music = "AlmostBliss";
                PlayerPrefs.SetInt("DropDownMusic", val);
                break;
            case 1:
                MenuManager.Music = "Floating";
                PlayerPrefs.SetInt("DropDownMusic", val);
                break;
            case 2:
                PlayerPrefs.SetInt("DropDownMusic", val);
                MenuManager.Music = "Neverland";
                break;
            case 3:
                PlayerPrefs.SetInt("DropDownMusic", val);
                MenuManager.Music = "Breathtaking";
                break;
            case 4:
                PlayerPrefs.SetInt("DropDownMusic", val);
                MenuManager.Music = "Smores";
                break;
            case 5:
                PlayerPrefs.SetInt("DropDownMusic", val);
                MenuManager.Music = "Nature";
                break;
            default:
                MenuManager.Music = "Nature";
                break;
        }
    }

    public void MusicChange()
    {
        FindObjectOfType<AudioManager>().Stop(MenuManager.Music);
    }

    public void SetVolume(float volume)
    {
        FindObjectOfType<AudioManager>().Play(MenuManager.Music);
        audioMixer.SetFloat("GameMusicVolume", volume);
    }
}
