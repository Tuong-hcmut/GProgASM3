using UnityEngine;
using TMPro;
using System.Collections;

public class SaveNotificationUI : MonoBehaviour
{
    public TMP_Text notificationText;
    public float fadeDuration = 2f; // thời gian mờ dần
    private Coroutine fadeCoroutine;

    void Start()
    {
        if (notificationText != null)
            notificationText.alpha = 0; // ẩn khi bắt đầu
    }

    public void ShowNotification(string message)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeNotification(message));
    }

    private IEnumerator FadeNotification(string message)
    {
        notificationText.text = message;

        // Hiện nhanh
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            notificationText.alpha = Mathf.Lerp(0, 1, t / 0.3f);
            yield return null;
        }

        // Giữ 1 lúc
        yield return new WaitForSeconds(1f);

        // Mờ dần biến mất
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            notificationText.alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            yield return null;
        }
    }
}
