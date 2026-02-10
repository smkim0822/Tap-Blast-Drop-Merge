using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Referernces")]
    public TextMeshProUGUI currentScoreText;    // 현재 점수 UI
    public TextMeshProUGUI bestScoreText;       // 최고 점수 UI

    [Header("Punch Animation")]
    public float punchScale = 1.25f;
    public float duration = 0.15f;

    [Header("Best Score Animation")]
    public float bestPunchScale = 1.3f;

    private int currentScore = 0;
    private int bestScore = 0;

    private Vector3 originalScale;
    private Vector3 bestScoreOriginalScale;
    private Coroutine punchCoroutine;
    private Coroutine bestPunchCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 원래 크기를 미리 저장
        if (currentScoreText != null)
        {
            originalScale = currentScoreText.transform.localScale;
        }
        if (bestScoreText != null)
        {
            bestScoreOriginalScale = bestScoreText.transform.localScale;
        }
    }

    void Start()
    {
        // 저장된 최고 점수 불러오기
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        bool isNewRecord = false;

        // 최고 기록 경신 체크 및 저장
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
            isNewRecord = true;
        }

        UpdateUI();

        // 현재 점수 펀치 애니메이션
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
        }
        punchCoroutine = StartCoroutine(CoPunchText(currentScoreText, originalScale, punchScale));

        // 최고 점수 펀치 애니메이션
        if (isNewRecord)
        {
            if (bestPunchCoroutine != null)
            {
                StopCoroutine(bestPunchCoroutine);
            }
            
            bestPunchCoroutine = StartCoroutine(CoPunchText(bestScoreText, bestScoreOriginalScale, bestPunchScale, Color.yellow));
        }
    }

    IEnumerator CoPunchText(TextMeshProUGUI targetText, Vector3 baseScale, float scaleMult, Color? flashColor = null)
    {
        if (targetText == null)
        {
            yield break;
        }

        Color originalColor = targetText.color;
        targetText.transform.localScale = baseScale * scaleMult;
        if (flashColor.HasValue)
        {
            targetText.color = flashColor.Value;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // Lerp를 사용하여 부드럽게 보간
            targetText.transform.localScale = Vector3.Lerp(baseScale * scaleMult, baseScale, percent);

            if (flashColor.HasValue)
            {
                targetText.color = Color.Lerp(flashColor.Value, originalColor, percent);
            }

            yield return null;
        }

        // 3. 마지막에 정확한 크기로 고정
        targetText.transform.localScale = baseScale;
        targetText.color = originalColor;
    }

    // 점수 초기화
    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentScoreText != null) currentScoreText.text = currentScore.ToString();

        if (bestScoreText != null) bestScoreText.text = "Best: " + bestScore.ToString();
    }

    public int GetCurrentScore() => currentScore;
}
