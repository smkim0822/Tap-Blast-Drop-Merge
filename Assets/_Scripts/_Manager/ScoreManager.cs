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
    private Color bestScoreOriginalColor = Color.white; // 최고 점수 원래 색상 저장용

    private Coroutine punchCoroutine;
    private Coroutine bestPunchCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 시작 시 원래 크기/색상을 미리 저장
        if (currentScoreText != null)
        {
            originalScale = currentScoreText.transform.localScale;
        }
        if (bestScoreText != null)
        {
            bestScoreOriginalScale = bestScoreText.transform.localScale;
            bestScoreOriginalColor = bestScoreText.color; // 원래 색 저장 (보통 흰색)
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

        // 1. 현재 점수 펀치 (크기만)
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(CoPunchScaleOnly(currentScoreText, originalScale, punchScale));

        // 2. 최고 점수 처리
        if (isNewRecord)
        {
            // 신기록이면 색상을 노란색으로 영구 변경 (이번 게임 동안)
            if (bestScoreText != null) bestScoreText.color = Color.yellow;

            // 펀치 애니메이션 (크기만)
            if (bestPunchCoroutine != null) StopCoroutine(bestPunchCoroutine);
            bestPunchCoroutine = StartCoroutine(CoPunchScaleOnly(bestScoreText, bestScoreOriginalScale, bestPunchScale));
        }
    }

    // 색상은 건드리지 않고 크기만 튕기는 코루틴
    IEnumerator CoPunchScaleOnly(TextMeshProUGUI targetText, Vector3 baseScale, float scaleMult)
    {
        if (targetText == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / duration;

            // 커졌다가 작아짐
            targetText.transform.localScale = Vector3.Lerp(baseScale * scaleMult, baseScale, percent);

            yield return null;
        }

        // 정확한 크기 복구
        targetText.transform.localScale = baseScale;
    }

    // 점수 초기화 (재시작 시 호출됨)
    public void ResetScore()
    {
        currentScore = 0;
        
        // 코루틴 모두 중단
        StopAllCoroutines();

        // UI 즉시 원상복구 (크기 및 색상)
        if (currentScoreText != null) 
        {
            currentScoreText.transform.localScale = originalScale;
        }
        if (bestScoreText != null) 
        {
            bestScoreText.transform.localScale = bestScoreOriginalScale;
            bestScoreText.color = bestScoreOriginalColor; // 여기서 원래 색(흰색)으로 돌아옴!
        }

        UpdateUI();
    }

    public void ReloadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentScoreText != null) currentScoreText.text = currentScore.ToString();

        if (bestScoreText != null) bestScoreText.text = "Best Score\n" + bestScore.ToString();
    }

    public int GetCurrentScore() => currentScore;
}