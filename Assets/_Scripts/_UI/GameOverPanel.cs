using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameOverPanel : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI resultLabel;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI bestScoreText;

    [Header("Buttons")]
    public Button retryButton;
    public CanvasGroup retryButtonCanvasGroup; // 버튼 투명도 조절용

    [Header("Animation Settings")]
    public float delayDuration = 1.0f; // 지연 시간
    public AnimationCurve appearanceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Popups")]
    public RankingPopup rankingPopup;

    private Coroutine retryEnableCoroutine;

    void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }
    }

    void OnRetryClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryGame();
        }
    }

    public void Setup(int currentScore, int bestScore, bool isNewRecord)
    {
        // 1. 점수 표기
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScore.ToString("N0");
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = bestScore.ToString("N0");
        }

        // 2. 패널 활성화
        gameObject.SetActive(true);

        // 3. 재시작 버튼 지연 애니메이션 시작
        if (retryEnableCoroutine != null) StopCoroutine(retryEnableCoroutine);
        retryEnableCoroutine = StartCoroutine(AnimateRetryButton());
    }

private IEnumerator AnimateRetryButton()
    {
        // 초기 상태 설정: 버튼 비활성화 및 크기/알파 0
        retryButton.interactable = false;
        retryButton.transform.localScale = Vector3.zero;
        if (retryButtonCanvasGroup != null) 
            retryButtonCanvasGroup.alpha = 0f;

        float timer = 0f;
        
        while (timer < delayDuration)
        {
            timer += Time.unscaledDeltaTime; 
            
            float progress = Mathf.Clamp01(timer / delayDuration); // 1을 넘지 않도록 Clamp
            float curveValue = appearanceCurve.Evaluate(progress);

            // 크기 및 투명도 애니메이션
            retryButton.transform.localScale = Vector3.one * curveValue;
            if (retryButtonCanvasGroup != null) 
                retryButtonCanvasGroup.alpha = curveValue;

            yield return null;
        }

        // 최종 상태 보장 (애니메이션 끝난 후 확실하게 1로 설정)
        retryButton.transform.localScale = Vector3.one;
        if (retryButtonCanvasGroup != null) 
            retryButtonCanvasGroup.alpha = 1f;
            
        retryButton.interactable = true;
    }

    public void OnClickRankingButton()
    {
        if (rankingPopup != null)
        {
            rankingPopup.OpenPopup();
        }
    }
}