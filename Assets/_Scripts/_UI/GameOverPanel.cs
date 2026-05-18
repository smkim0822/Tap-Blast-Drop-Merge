using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 게임 오버 시 결과 화면을 제어하는 UI 패널 스크립트.
/// 최종 점수 표시, 최고 기록 표시, 재시작 호출 기능 담당.
/// </summary>
public class GameOverPanel : MonoBehaviour
{
    [Header("UI Components (텍스트 요소)")]
    [Tooltip("결과 텍스트")]
    public TextMeshProUGUI resultLabel;
    [Tooltip("이번 판에 획득한 현재 점수")]
    public TextMeshProUGUI currentScoreText;
    [Tooltip("로컬에 저장된 최고 점수")]
    public TextMeshProUGUI bestScoreText;

    [Header("Buttons (버튼 요소)")]
    [Tooltip("게임을 다시 시작하는 버튼")]
    public Button retryButton;
    [Tooltip("재시작 버튼의 투명도를 자연스럽게 조절하기 위한 캔버스 그룹")]
    public CanvasGroup retryButtonCanvasGroup;

    [Header("Animation Settings (애니메이션 설정)")]
    [Tooltip("게임 오버 창이 뜨고 재시작 버튼이 나타나기까지의 대기 시간")]
    public float appearDelay = 0.5f;
    [Tooltip("재시작 버튼이 커지며 나타나는 연출의 진행 시간")]
    public float animDuration = 0.3f;
    [Tooltip("버튼이 나타날 때의 연출 그래프")]
    public AnimationCurve appearanceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine retryEnableCoroutine;

    void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }
    }

    void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryClicked);
        }
    }

    /// <summary>
    /// 재시작 버튼 클릭 시 호출.
    /// GameManager를 통해 게임 상태 리셋.
    /// </summary>
    void OnRetryClicked()
    {
        SessionEvents.OnRequestRetry?.Invoke();
    }

    /// <summary>
    /// 게임 오버가 되었을 때 외부에서 호출하여 패널을 초기화하고 화면에 뛰움.
    /// </summary>
    /// <param name="currentScore">이번 게임에서 얻은 점수</param>
    /// <param name="bestScore">기기 내 최고 점수</param>
    /// <param name="isNewRecord">최고 점수 갱신 여부</param>
    public void Setup(int currentScore, int bestScore, bool isNewRecord)
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScore.ToString("N0");
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = bestScore.ToString("N0");
        }

        gameObject.SetActive(true);

        if (retryEnableCoroutine != null)
        {
            StopCoroutine(retryEnableCoroutine);
        }
        retryEnableCoroutine = StartCoroutine(AnimateRetryButton());
    }

    private IEnumerator AnimateRetryButton()
    {
        retryButton.interactable = false;
        retryButton.transform.localScale = Vector3.zero;
        if (retryButtonCanvasGroup != null)
        {
            retryButtonCanvasGroup.alpha = 0f;
        }

        yield return new WaitForSecondsRealtime(appearDelay);

        float timer = 0f;
        while (timer < animDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / animDuration);
            float curveValue = appearanceCurve.Evaluate(progress);

            retryButton.transform.localScale = Vector3.one * curveValue;
            if (retryButtonCanvasGroup != null)
            {
                retryButtonCanvasGroup.alpha = curveValue;
            }

            yield return null;
        }

        retryButton.transform.localScale = Vector3.one;
        if (retryButtonCanvasGroup != null)
        {
            retryButtonCanvasGroup.alpha = 1f;
        }

        retryButton.interactable = true;
    }
}
