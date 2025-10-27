using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Settings : MonoBehaviour
{
    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI volume_text;
    private int currentVolume = 100;

    public TextMeshProUGUI sfx_text;

    private int currentSFX = 100;
    private Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1280, 720),
        new Vector2Int(800, 600)
    };

    private int currentIndex = 0;

    void Start()
    {
        UpdateResolutionLabel();
        UpdateVolumeLabel();
        ApplyVolume();
    }

    public void NextResolution()
    {
        currentIndex++;
        if (currentIndex >= resolutions.Length) currentIndex = 0;
        ApplyResolution();
    }

    public void PreviousResolution()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = resolutions.Length - 1;
        ApplyResolution();
    }

    private void ApplyResolution()
    {
        Screen.SetResolution(resolutions[currentIndex].x, resolutions[currentIndex].y, FullScreenMode.Windowed);
        UpdateResolutionLabel();
    }

    private void UpdateResolutionLabel()
    {
        resolutionText.text = resolutions[currentIndex].x + " x " + resolutions[currentIndex].y;
    }



    public void IncreaseVolume()
    {
        currentVolume += 10;
        if (currentVolume > 100) currentVolume = 100;
        ApplyVolume();
    }

    public void DecreaseVolume()
    {
        currentVolume -= 10;
        if (currentVolume < 0) currentVolume = 0;
        ApplyVolume();
    }

    private void ApplyVolume()
    {
        // Âm lượng hệ thống (AudioListener.volume từ 0.0 -> 1.0)
        AudioListener.volume = currentVolume / 100f;
        UpdateVolumeLabel();
    }

    private void UpdateVolumeLabel()
    {
        volume_text.text = currentVolume.ToString() + "%";
    }
    
    public void IncreaseSFX()
    {
        currentSFX += 10;
        if (currentSFX > 100) currentSFX = 100;
        ApplySFX();
    }

    public void DecreaseSFX()
    {
        currentSFX -= 10;
        if (currentSFX < 0) currentSFX = 0;
        ApplySFX();
    }

    private void ApplySFX()
    {
        // Âm lượng hệ thống (AudioListener.volume từ 0.0 -> 1.0)
        float sfxVolume = currentSFX / 100f;
        UpdateSFXLabel();

    }

    private void UpdateSFXLabel()
    {
        sfx_text.text = currentSFX.ToString() + "%";
    }
}
