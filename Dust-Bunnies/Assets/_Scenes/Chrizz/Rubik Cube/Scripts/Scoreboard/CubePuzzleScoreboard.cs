using TMPro;
using UnityEngine;
using System.Collections;

public class CubePuzzleScoreboard : MonoBehaviour {
    [SerializeField] private CanvasGroup canvas;
    [SerializeField] private RectTransform uiRect;
    [SerializeField] private CubePuzzleInteractable puzzle;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private Color solveColor;

    private float _internalTime = 0f;
    private bool _isActive = false;
    private bool _solved = false;

    private Vector3 _originalPos;

    private void Awake() {
        puzzle.OnInteractAction += Toggle;
        puzzle.OnSolved += Solve;
        _originalPos = uiRect.anchoredPosition;
    }

    private void Solve() {
        _solved = true;
        text.color = solveColor;
    }

    private void Toggle(bool isActive) {
        _isActive = isActive;

        StartCoroutine(
            isActive ? FadeInAndSlide(canvas, uiRect, fadeSpeed) 
                : FadeOutAndSlide(canvas, uiRect, fadeSpeed));
    }

    private void Update() {
        if (!_isActive || _solved) return;

        int minutes = Mathf.FloorToInt(_internalTime / 60f);
        int seconds = Mathf.FloorToInt(_internalTime % 60f);
        string mm_ss = $"{minutes:00}:{seconds:00}";
        
        text.SetText(mm_ss);
        
        _internalTime += Time.deltaTime;
    }
    
    public IEnumerator FadeInAndSlide(
        CanvasGroup canvasGroup,
        RectTransform rect,
        float duration,
        float startOffsetY = -200f
    ) {
        Vector2 originalPos = _originalPos;
        Vector2 startPos = originalPos + new Vector2(0, startOffsetY);

        float elapsed = 0f;

        canvasGroup.alpha = 0f;
        rect.anchoredPosition = startPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float eased = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = eased;

            rect.anchoredPosition = Vector2.Lerp(startPos, originalPos, eased);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rect.anchoredPosition = originalPos;
    }
    
    public IEnumerator FadeOutAndSlide(
        CanvasGroup canvasGroup,
        RectTransform rect,
        float duration,
        float endOffsetY = -200f
    ) 
    {
        Vector2 originalPos = _originalPos;
        Vector2 endPos = originalPos + new Vector2(0, endOffsetY);

        float elapsed = 0f;

        canvasGroup.alpha = 1f;
        rect.anchoredPosition = originalPos;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float eased = Mathf.SmoothStep(0f, 1f, t);

            canvasGroup.alpha = 1f - eased;

            rect.anchoredPosition = Vector2.Lerp(originalPos, endPos, eased);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        rect.anchoredPosition = endPos;
    }
}
