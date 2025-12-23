using UnityEngine;

public class EndCredits : MonoBehaviour
{
    public float speed = 40f;
    public GameObject thanksText;
    public float fadeDuration = 1.5f;
    public float showDuration = 2f;

    private RectTransform rectTransform;
    private bool hasShownThanks = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (thanksText != null)
        {
            var cg = thanksText.GetComponent<CanvasGroup>();
            if (cg == null)
                thanksText.AddComponent<CanvasGroup>();
            thanksText.GetComponent<CanvasGroup>().alpha = 0f;
        }
    }

    void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * speed * Time.deltaTime;
        if (!hasShownThanks && rectTransform.anchoredPosition.y > 211f)
        {
            hasShownThanks = true;
            StartCoroutine(ShowThanks());
        }
    }

    private System.Collections.IEnumerator ShowThanks()
    {
        var cg = thanksText.GetComponent<CanvasGroup>();
        // Fade in
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;

        // Wait
        yield return new WaitForSeconds(showDuration);

        // Fade out
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        SaveManager.ClearSave();
        Application.Quit();
    }
}