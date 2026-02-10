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

    private int currentScore = 0;
    private int bestScore = 0;

    private Vector3 originalScale;
    private Coroutine punchCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 원래 크기를 미리 저장
        if (currentScoreText != null)
        {
            originalScale = currentScoreText.transform.localScale;
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

        // 최고 기록 경신 체크 및 저장
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        UpdateUI();

        // 펀치 애니메이션 실행
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
        }
        punchCoroutine = StartCoroutine(CoPunchText());
    }

    IEnumerator CoPunchText()
    {
        if (currentScoreText == null)
        {
            yield break;
        }

        // 1. 즉시 커짐
        currentScoreText.transform.localScale = originalScale * punchScale;

        // 2. duration 동안 부드럽게 원래 크기로 복구
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // Lerp를 사용하여 부드럽게 보간
            currentScoreText.transform.localScale = Vector3.Lerp(originalScale * punchScale, originalScale, percent);
            yield return null;
        }

        // 3. 마지막에 정확한 크기로 고정
        currentScoreText.transform.localScale = originalScale;
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
