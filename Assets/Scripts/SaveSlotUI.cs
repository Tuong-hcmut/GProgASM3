// SaveSlotUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SaveSlotUI : MonoBehaviour
{
    public Button loadButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI infoText;
    public Button deleteButton;

    private int slotIndex = 0;

    private void Awake()
    {
        if (loadButton != null) loadButton.onClick.AddListener(OnLoadClicked);
        if (deleteButton != null) deleteButton.onClick.AddListener(OnDeleteClicked);
    }

    public void Refresh(SaveData sd, int index)
    {
        slotIndex = index;
        if (sd == null)
        {
            titleText.text = $"Slot {index + 1}:";
            infoText.text = "No save";
            loadButton.interactable = false;
            if (deleteButton) deleteButton.gameObject.SetActive(false);
        }
        else
        {
            titleText.text = $"Slot {index + 1}: ";
            var pos = sd.GetPosition();
            infoText.text = $"Time: {sd.GetTimestampString()}";
            loadButton.interactable = true;
            if (deleteButton) deleteButton.gameObject.SetActive(true);
        }
    }

    public void OnLoadClicked()
    {
        Debug.Log($"Request load slot {slotIndex+1}");
        SaveManager.Instance.LoadSlotAndStart(slotIndex);
    }

    public void OnDeleteClicked()
    {
        SaveManager.Instance.DeleteSlot(slotIndex);
        // Refresh UI (attempt to call Main Menu's UpdateLoadPanel)
        var mm = FindFirstObjectByType<Main_Menu_Controller>();
        if (mm) mm.CloseLoadPanel(); // (simple) close and re-open to refresh
        if (mm) mm.LoadGameButton();
    }
}
