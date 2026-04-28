using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// 점수 HUD의 텍스트 갱신과 펀치 애니메이션을 담당합니다.
/// 점수 상태 계산은 ScoreManager/ScoreState가 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class ScoreHudView : MonoBehaviour
{
    [Header("UI References (UI 참조)")]
    [Tooltip("현재 점수를 표시할 TextMeshPro UI")]
    [SerializeField] TextMeshProUGUI currentScoreText;
    [Tooltip("최고 점수를 표시할 TextMeshPro UI")]
    [SerializeField] TextMeshProUGUI bestScoreText;

    [Header("Punch Animation (현재 점수 펀치 애니메이션)")]
    [Tooltip("점수 획득 시 텍스트가 커질 배율")]
    [SerializeField] float punchScale = 1.25f;
    [Tooltip("애니메이션 진행 시간 (초)")]
    [SerializeField] float duration = 0.15f;

    [Header("Best Score Animation (최고 점수 펀치 애니메이션)")]
    [Tooltip("최고 기록 갱신 시 텍스트가 커질 배율")]
    [SerializeField] float bestPunchScale = 1.3f;

    Vector3 originalScale;
    Vector3 bestScoreOriginalScale;
    Color bestScoreOriginalColor = Color.white;

    Coroutine punchCoroutine;
    Coroutine bestPunchCoroutine;

    void Awake()
    {
        CacheOriginalState();
    }

    public void Render(int currentScore, int bestScore)
    {
        if (currentScoreText != null) currentScoreText.text = currentScore.ToString();
        if (bestScoreText != null) bestScoreText.text = "Best Score\n" + bestScore.ToString();
    }

    public void PlayScoreAdded(bool isNewRecord)
    {
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(CoPunchScaleOnly(currentScoreText, originalScale, punchScale));

        if (!isNewRecord) return;

        if (bestScoreText != null) bestScoreText.color = Color.yellow;

        if (bestPunchCoroutine != null) StopCoroutine(bestPunchCoroutine);
        bestPunchCoroutine = StartCoroutine(CoPunchScaleOnly(bestScoreText, bestScoreOriginalScale, bestPunchScale));
    }

    public void ResetVisualState()
    {
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        if (bestPunchCoroutine != null) StopCoroutine(bestPunchCoroutine);

        if (currentScoreText != null)
        {
            currentScoreText.transform.localScale = originalScale;
        }

        if (bestScoreText != null)
        {
            bestScoreText.transform.localScale = bestScoreOriginalScale;
            bestScoreText.color = bestScoreOriginalColor;
        }
    }

    void CacheOriginalState()
    {
        if (currentScoreText != null)
        {
            originalScale = currentScoreText.transform.localScale;
        }

        if (bestScoreText != null)
        {
            bestScoreOriginalScale = bestScoreText.transform.localScale;
            bestScoreOriginalColor = bestScoreText.color;
        }
    }

    IEnumerator CoPunchScaleOnly(TextMeshProUGUI targetText, Vector3 baseScale, float scaleMult)
    {
        if (targetText == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;
            targetText.transform.localScale = Vector3.Lerp(baseScale * scaleMult, baseScale, percent);
            yield return null;
        }

        targetText.transform.localScale = baseScale;
    }
}
