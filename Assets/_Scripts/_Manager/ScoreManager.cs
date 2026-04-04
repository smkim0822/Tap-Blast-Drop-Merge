using UnityEngine;
using TMPro;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI References (UI 참조)")]
    [Tooltip("현재 점수를 표시할 TextMeshPro UI")]
    public TextMeshProUGUI currentScoreText;
    [Tooltip("최고 점수를 표시할 TestMeshPro UI")]
    public TextMeshProUGUI bestScoreText;

    [Header("Punch Animation (현재 점수 펀치 애니메이션)")]
    [Tooltip("점수 획득 시 텍스트가 커질 배율")]
    public float punchScale = 1.25f;
    [Tooltip("애니메이션 진행 시간 (초)")]
    public float duration = 0.15f;

    [Header("Best Score Animation (최고 점수 펀치 애니메이션)")]
    [Tooltip("최고 기록 갱신 시 텍스트가 커질 배율")]
    public float bestPunchScale = 1.3f;

    // 내부 점수 데이터
    private int currentScore = 0;
    private int bestScore = 0;

    // UI 원본 상태 저장용 변수
    private Vector3 originalScale;
    private Vector3 bestScoreOriginalScale;
    private Color bestScoreOriginalColor = Color.white; // 최고 점수 원래 색상 저장용

    // 진행 중인 코루틴 추적용 변수
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
            bestScoreOriginalColor = bestScoreText.color; // 원래 색 저장
        }
    }

    /// <summary>
    /// 오브젝트 활성화 시 점수 획득 이벤트 구독
    /// </summary>
    private void OnEnable()
    {
        GameEvents.OnScoreAdded += AddScore;
    }

    /// <summary>
    /// 오브젝트 비활성화 시 점수 획득 이벤트 해제
    /// </summary>
    private void OnDisable()
    {
        GameEvents.OnScoreAdded -= AddScore;
    }

    void Start()
    {
        // 저장된 최고 점수 불러오기
        bestScore = PlayerPrefs.GetInt("BestScore", 0);

        // UI 갱신
        UpdateUI();
    }

    /// <summary>
    /// 점수를 획득하고 UI 및 애니메이션 업데이트
    /// 신기록 달성 시 최고 점수 UI도 함께 업데이트 및 애니메이션 처리
    /// </summary>
    /// <param name="amount"></param>
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

        // UI 텍스트 갱신
        UpdateUI();

        // 현재 점수 UI 펀치
        if (punchCoroutine != null) StopCoroutine(punchCoroutine);
        punchCoroutine = StartCoroutine(CoPunchScaleOnly(currentScoreText, originalScale, punchScale));

        // 최고 점수 처리
        if (isNewRecord)
        {
            // 신기록이면 색상을 노란색으로 영구 변경 (이번 게임 동안)
            if (bestScoreText != null) bestScoreText.color = Color.yellow;

            // 최고 점수 UI 펀치
            if (bestPunchCoroutine != null) StopCoroutine(bestPunchCoroutine);
            bestPunchCoroutine = StartCoroutine(CoPunchScaleOnly(bestScoreText, bestScoreOriginalScale, bestPunchScale));
        }
    }

    /// <summary>
    /// 텍스트 펀치 애니메이션 코루틴
    /// </summary>
    /// <param name="targetText"></param>
    /// <param name="baseScale"></param>
    /// <param name="scaleMult"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 점수 초기화 (재시작 시 호출됨)
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        
        // 진행 중인 애니메이션 코루틴 중단
        if (punchCoroutine != null)
        {
            StopCoroutine(punchCoroutine);
        }
        if (bestPunchCoroutine != null)
        {
            StopCoroutine(bestPunchCoroutine);
        }

        // UI 즉시 원상복구 (크기 및 색상)
        if (currentScoreText != null) 
        {
            currentScoreText.transform.localScale = originalScale;
        }
        if (bestScoreText != null) 
        {
            bestScoreText.transform.localScale = bestScoreOriginalScale;
            bestScoreText.color = bestScoreOriginalColor;
        }

        UpdateUI();
    }

    /// <summary>
    /// 서버 데이터 동기화 등으로 인해 외부에서 최고 점수를 다시 불러와야 할 때 호출
    /// </summary>
    public void ReloadBestScore()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateUI();
    }

    /// <summary>
    /// 현재 점수와 최고 점수 UI의 텍스트 갱신
    /// </summary>
    void UpdateUI()
    {
        if (currentScoreText != null) currentScoreText.text = currentScore.ToString();

        if (bestScoreText != null) bestScoreText.text = "Best Score\n" + bestScore.ToString();
    }

    /// <summary>
    /// 외부에서 현재 점수를 가져갈 때 사용
    /// </summary>
    /// <returns></returns>
    public int GetCurrentScore() => currentScore;
}