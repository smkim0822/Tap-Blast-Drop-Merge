using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 행 전체를 길게 누르면 점수를 전체 숫자로 표시합니다. 기본은 축약 표기입니다.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class RankingScorePressable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private const float FullScoreWidthBonus = 56f;
    private const float FullScoreInsetReduction = 8f;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private float longPressSeconds = 0.4f;

    private int score;
    private bool showingFullScore;
    private Coroutine longPressRoutine;
    private RectTransform scoreRect;
    private Vector2 defaultScoreSize;
    private Vector2 defaultScorePosition;
    private bool hasCachedLayout;

    private void Awake()
    {
        Image rowImage = GetComponent<Image>();
        if (rowImage != null)
        {
            rowImage.raycastTarget = true;
        }
    }

    public void Bind(TextMeshProUGUI targetScoreText)
    {
        scoreText = targetScoreText;
        scoreRect = scoreText != null ? scoreText.rectTransform : null;
        CacheScoreLayout();
        ApplyTextStyle();
    }

    public void SetScore(int value)
    {
        score = Mathf.Max(0, value);
        showingFullScore = false;
        CancelLongPress();
        ApplyScoreLayout();
        ApplyDisplayText();
        ApplyTextStyle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!RankingScoreFormat.UseCompactDisplay(score))
        {
            return;
        }

        CancelLongPress();
        longPressRoutine = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        EndLongPressPreview();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EndLongPressPreview();
    }

    private IEnumerator LongPressRoutine()
    {
        yield return new WaitForSecondsRealtime(longPressSeconds);
        showingFullScore = true;
        ApplyScoreLayout();
        ApplyDisplayText();
        longPressRoutine = null;
    }

    private void EndLongPressPreview()
    {
        CancelLongPress();
        if (!showingFullScore)
        {
            return;
        }

        showingFullScore = false;
        ApplyScoreLayout();
        ApplyDisplayText();
    }

    private void CancelLongPress()
    {
        if (longPressRoutine == null)
        {
            return;
        }

        StopCoroutine(longPressRoutine);
        longPressRoutine = null;
    }

    private void CacheScoreLayout()
    {
        if (scoreRect == null || hasCachedLayout)
        {
            return;
        }

        defaultScoreSize = scoreRect.sizeDelta;
        defaultScorePosition = scoreRect.anchoredPosition;
        hasCachedLayout = true;
    }

    private void ApplyScoreLayout()
    {
        if (scoreRect == null)
        {
            return;
        }

        CacheScoreLayout();

        if (showingFullScore)
        {
            scoreRect.sizeDelta = new Vector2(defaultScoreSize.x + FullScoreWidthBonus, defaultScoreSize.y);
            scoreRect.anchoredPosition = new Vector2(
                defaultScorePosition.x + FullScoreInsetReduction,
                defaultScorePosition.y);
        }
        else
        {
            scoreRect.sizeDelta = defaultScoreSize;
            scoreRect.anchoredPosition = defaultScorePosition;
        }
    }

    private void ApplyDisplayText()
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.text = showingFullScore
            ? RankingScoreFormat.FormatFull(score)
            : RankingScoreFormat.FormatDisplay(score);
    }

    private void ApplyTextStyle()
    {
        if (scoreText == null)
        {
            return;
        }

        scoreText.textWrappingMode = TextWrappingModes.NoWrap;
        scoreText.overflowMode = TextOverflowModes.Overflow;
        scoreText.horizontalAlignment = HorizontalAlignmentOptions.Right;
    }
}
