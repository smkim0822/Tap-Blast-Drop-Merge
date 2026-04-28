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
    [Tooltip("랭킹 창을 여는 버튼")]
    public Button rankingButton;
    [Tooltip("재시작 버튼의 투명도를 자연스럽게 조절하기 위한 캔버스 그룹")]
    public CanvasGroup retryButtonCanvasGroup;

    [Header("Animation Settings (애니메이션 설정)")]
    [Tooltip("게임 오버 창이 뜨고 재시작 버튼이 나타나기까지의 대기 시간")]
    public float appearDelay = 0.5f;
    [Tooltip("재시작 버튼이 커지며 나타나는 연출의 진행 시간")]
    public float animDuration = 0.3f;
    [Tooltip("버튼이 나타날 때의 연출 그래프")]
    public AnimationCurve appearanceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Popups (팝업 연결)")]
    [Tooltip("랭킹 버튼을 눌렀을 때 띄워줄 랭킹 팝업 객체")]
    public RankingPopup rankingPopup;

    private Coroutine retryEnableCoroutine;

    void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }
        if (rankingButton != null)
        {
            rankingButton.onClick.AddListener(OnClickRankingButton);
        }
    }

    void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryClicked);
        }
        if (rankingButton != null)
        {
            rankingButton.onClick.RemoveListener(OnClickRankingButton);
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
        if (retryEnableCoroutine != null) 
        {
            StopCoroutine(retryEnableCoroutine);
        }
        retryEnableCoroutine = StartCoroutine(AnimateRetryButton());
    }

    /// <summary>
    /// 재시작 버튼이 바로 눌리지 않도록 약간의 딜레이 후 버튼을 애니메이션과 함께 나타나게 함.
    /// </summary>
    /// <returns></returns>
    private IEnumerator AnimateRetryButton()
    {
        // 초기 상태 설정: 버튼 비활성화 및 크기/알파 0
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
            
            float progress = Mathf.Clamp01(timer / animDuration); // 1을 넘지 않도록 Clamp
            float curveValue = appearanceCurve.Evaluate(progress);

            // 크기 및 투명도 애니메이션
            retryButton.transform.localScale = Vector3.one * curveValue;
            if (retryButtonCanvasGroup != null)
            {
                retryButtonCanvasGroup.alpha = curveValue;
            }

            yield return null;
        }

        // 최종 상태 보장 (애니메이션 끝난 후 확실하게 1로 설정)
        retryButton.transform.localScale = Vector3.one;
        if (retryButtonCanvasGroup != null) 
        {
            retryButtonCanvasGroup.alpha = 1f;
        }

        // 터치 활성화
        retryButton.interactable = true;
    }

    /// <summary>
    /// 랭킹 버튼 클릭 시 랭킹 팝업을 엶.
    /// </summary>
    public void OnClickRankingButton()
    {
        if (rankingPopup != null)
        {
            rankingPopup.OpenPopup();
        }
    }
}