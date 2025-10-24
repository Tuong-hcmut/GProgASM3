using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Main_Menu_Controller : MonoBehaviour
{
    public GameObject modeSelectPanel;
    public GameObject mainMenuPanel;

    public GameObject settingMenuPanel;
    void Start()
    {
    Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
    }

    public void OpenModeSelect()
    {
        modeSelectPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseModeSelect()
    {
        modeSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OpenSettingSelect()
    {
        settingMenuPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseSettingSelect()
    {
        settingMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ChoosePVP()
    {
        Debug.Log("Choose Player vs Player");
        SceneManager.LoadScene("Player_vs_Player");
    }

    public void ChoosePVC()
    {
        Debug.Log("Choose Player vs AI");
        SceneManager.LoadScene("Player_vs_Computer");
    }
    // public void LoadScene(string sceneName)
    // {
    //     SceneManager.LoadScene(sceneName);
    // }

    // public void LoadSceneByIndex(int index)
    // {
    //     SceneManager.LoadScene(index);
    // }

   public void QuitGame()
{
    Debug.Log("Game exited.");
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false; 
#else
    Application.Quit(); 
#endif
}

}
