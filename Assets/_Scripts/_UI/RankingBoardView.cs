using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 랭킹 팝업의 내 순위/랭킹 리스트/오프라인 표시 렌더링을 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class RankingBoardView : MonoBehaviour
{
    private const int MaxNicknameLength = 10;

    [Header("UI References (기본 UI 연결)")]
    [SerializeField] Transform contentParent;
    [SerializeField] ScrollRect rankingScrollRect;
    [SerializeField] RankingItem itemPrefab;
    [SerializeField] GameObject offlineMessageObject;
    [SerializeField] GameObject loadingMessageObject;

    [Header("My Rank References (내 순위 UI)")]
    [SerializeField] TextMeshProUGUI myNameText;
    [SerializeField] TextMeshProUGUI myRankText;
    [SerializeField] TextMeshProUGUI myScoreText;

    private RankingScorePressable myScorePressable;
    private Coroutine scrollResetRoutine;
    private CanvasGroup contentLoadingCanvasGroup;

    private void Awake()
    {
        if (rankingScrollRect == null)
        {
            rankingScrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        ResolveMyRankReferences();
        EnsureMyScorePressable();
        HideStrayRankingItemPreview();
    }

    /// <summary>
    /// 서버 응답 전에도 저장된 로컬 닉네임을 My Rank에 즉시 표시합니다.
    /// </summary>
    public void RefreshLocalMyRank()
    {
        int localBest = ScoreManager.Instance != null
            ? ScoreManager.Instance.GetBestScore()
            : BestScoreStore.Load();

        UpdateMyRank(null, ResolveLocalNickname(), localBest);
    }

    public void ShowOffline(MyRankData myData, string displayName, int localBest)
    {
        SetLoading(false);
        ClearRankingItems();
        if (offlineMessageObject != null)
        {
            offlineMessageObject.SetActive(true);
        }

        UpdateMyRank(myData, displayName, localBest);
        ResetScrollToTop();
    }

    public void ShowOnline(MyRankData myData, string displayName, int localBest, ScoreData[] ranks)
    {
        SetLoading(false);
        if (offlineMessageObject != null)
        {
            offlineMessageObject.SetActive(false);
        }

        UpdateMyRank(myData, displayName, localBest);
        RenderRankingItems(ranks);
        ResetScrollToTop();
    }

    public void UpdateMyRank(MyRankData myData, string displayName, int localBest)
    {
        string finalName = BuildMyRankDisplayName(myData, displayName);

        if (myNameText != null)
        {
            myNameText.text = finalName;
            myNameText.color = RankingUiTheme.PrimaryText;
            ApplyMyRankNameStyle(myNameText);
        }

        if (myRankText != null)
        {
            myRankText.text = (myData != null && myData.rank > 0) ? myData.rank.ToString() : "-";
            myRankText.color = RankingUiTheme.PrimaryText;
            ApplyMyRankTextStyle(myRankText);
        }

        int displayScore = myData != null ? Mathf.Max(myData.score, localBest) : localBest;
        if (myScorePressable != null)
        {
            if (myScoreText != null)
            {
                myScoreText.color = RankingUiTheme.PrimaryText;
            }

            myScorePressable.SetScore(displayScore);
        }
        else if (myScoreText != null)
        {
            myScoreText.text = RankingScoreFormat.FormatDisplay(displayScore);
            myScoreText.color = RankingUiTheme.PrimaryText;
            ApplyMyRankTextStyle(myScoreText);
        }
    }

    public void RenderRankingItems(ScoreData[] ranks)
    {
        ClearRankingItems();

        if (ranks == null || ranks.Length == 0 || contentParent == null || itemPrefab == null)
        {
            return;
        }

        for (int i = 0; i < ranks.Length; i++)
        {
            RankingItem newItem = Instantiate(itemPrefab, contentParent);
            newItem.SetData(i + 1, ranks[i].name, ranks[i].score);
        }
    }

    public void ClearRankingItems()
    {
        if (contentParent == null) return;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetLoading(bool isLoading)
    {
        if (loadingMessageObject != null)
        {
            loadingMessageObject.SetActive(isLoading);
        }
        else if (contentParent != null)
        {
            // 씬에 Loading UI 오브젝트가 없을 때 리스트 영역으로 로딩 상태 표시
            if (contentLoadingCanvasGroup == null)
            {
                contentLoadingCanvasGroup = contentParent.GetComponent<CanvasGroup>();
                if (contentLoadingCanvasGroup == null)
                {
                    contentLoadingCanvasGroup = contentParent.gameObject.AddComponent<CanvasGroup>();
                }
            }

            contentLoadingCanvasGroup.alpha = isLoading ? 0.35f : 1f;
            contentLoadingCanvasGroup.interactable = !isLoading;
            contentLoadingCanvasGroup.blocksRaycasts = !isLoading;
        }

        if (isLoading && offlineMessageObject != null)
        {
            offlineMessageObject.SetActive(false);
        }
    }

    public void ResetScrollToTop()
    {
        if (scrollResetRoutine != null)
        {
            StopCoroutine(scrollResetRoutine);
        }

        scrollResetRoutine = StartCoroutine(ResetScrollToTopRoutine());
    }

    private IEnumerator ResetScrollToTopRoutine()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        if (rankingScrollRect == null)
        {
            yield break;
        }

        rankingScrollRect.velocity = Vector2.zero;
        rankingScrollRect.verticalNormalizedPosition = 1f;
    }

    private void HideStrayRankingItemPreview()
    {
        if (contentParent == null || itemPrefab == null)
        {
            return;
        }

        RankingItem[] items = GetComponentsInChildren<RankingItem>(true);
        for (int i = 0; i < items.Length; i++)
        {
            RankingItem item = items[i];
            if (item == itemPrefab)
            {
                continue;
            }

            if (item.transform.parent != contentParent)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    private void ResolveMyRankReferences()
    {
        if (myNameText != null)
        {
            return;
        }

        Transform myRankBar = transform.Find("Window/Board/MyRankBar");
        if (myRankBar == null)
        {
            myRankBar = transform.Find("Board/MyRankBar");
        }

        if (myRankBar == null)
        {
            return;
        }

        Transform nameText = myRankBar.Find("NameText");
        if (nameText == null)
        {
            nameText = myRankBar.Find("NameBox/NameText");
        }

        if (nameText != null)
        {
            myNameText = nameText.GetComponent<TextMeshProUGUI>();
        }

        if (myRankText == null)
        {
            Transform rankText = myRankBar.Find("RankText");
            if (rankText != null)
            {
                myRankText = rankText.GetComponent<TextMeshProUGUI>();
            }
        }

        if (myScoreText == null)
        {
            Transform scoreText = myRankBar.Find("ScoreText");
            if (scoreText != null)
            {
                myScoreText = scoreText.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void EnsureMyScorePressable()
    {
        if (myScoreText == null)
        {
            return;
        }

        Transform myRankBar = myScoreText.transform.parent;
        if (myRankBar == null || myRankBar.name != "MyRankBar")
        {
            myRankBar = transform.Find("Window/Board/MyRankBar");
        }

        if (myRankBar == null)
        {
            return;
        }

        myScorePressable = myRankBar.GetComponent<RankingScorePressable>();
        if (myScorePressable == null)
        {
            myScorePressable = myRankBar.gameObject.AddComponent<RankingScorePressable>();
        }

        myScorePressable.Bind(myScoreText);

        RankingScorePressable stale = myScoreText.GetComponent<RankingScorePressable>();
        if (stale != null && stale != myScorePressable)
        {
            Destroy(stale);
        }

        DisableRaycastForLongPress(myRankText);
        DisableRaycastForLongPress(myNameText);
        DisableRaycastForLongPress(myScoreText);
    }

    private static void DisableRaycastForLongPress(TextMeshProUGUI text)
    {
        if (text != null)
        {
            text.raycastTarget = false;
        }
    }

    private static string BuildMyRankDisplayName(MyRankData myData, string displayName)
    {
        if (myData == null && !string.IsNullOrEmpty(displayName) && displayName.Contains("Offline"))
        {
            return displayName;
        }

        string localName = ResolveLocalNickname();
        if (!string.IsNullOrEmpty(localName) && localName != "Guest_Error")
        {
            return TruncateNickname(localName);
        }

        if (myData != null && !string.IsNullOrEmpty(myData.name))
        {
            return TruncateNickname(myData.name);
        }

        if (!string.IsNullOrEmpty(displayName))
        {
            return TruncateNickname(displayName);
        }

        return string.Empty;
    }

    private static string ResolveLocalNickname()
    {
        if (NicknameManager.Instance != null)
        {
            return NicknameManager.Instance.GetNickname();
        }

        return NicknameStore.Load("Guest");
    }

    private static string TruncateNickname(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        return name.Length <= MaxNicknameLength ? name : name.Substring(0, MaxNicknameLength);
    }

    private static void ApplyMyRankTextStyle(TextMeshProUGUI text)
    {
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private static void ApplyMyRankNameStyle(TextMeshProUGUI text)
    {
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
    }
}
