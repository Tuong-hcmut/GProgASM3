using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Main_Menu_Controller : MonoBehaviour
{
    public GameObject mainMenuPanel;

    [Header("Setting Panel")]
    public GameObject settingMenuPanel;

    [Header("Load Panel")]
    public GameObject loadPanel;
    public SaveSlotUI[] slotUIs;

    void Start()
    {
        Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
        if (SaveManager.Instance == null)
        {
            var go = new GameObject("SaveManager");
            go.AddComponent<SaveManager>();
        }
    }

    public void EnterGame()
    {
        Debug.Log("Enter Game Successfully");
        SceneManager.LoadScene("Game_Scene");
    }

    public void LoadGameButton()
    {
        if (loadPanel != null)
        {
            loadPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
            UpdateLoadPanel();
        }
    }

    public void CloseLoadPanel()
    {
        if (loadPanel != null)
        {
            loadPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            SaveManager.Instance.LoadAllFromDisk();
        }
    }

    void UpdateLoadPanel()
    {
        if (SaveManager.Instance == null) return;
        var saves = SaveManager.Instance.GetCachedSaves();
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (i < saves.Length)
            {
                slotUIs[i].Refresh(saves[i], i);
            }
            else
            {
                slotUIs[i].Refresh(null, i);
            }
        }
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
