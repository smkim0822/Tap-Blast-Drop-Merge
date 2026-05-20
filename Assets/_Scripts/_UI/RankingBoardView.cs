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

    [Header("Debug (에디터 확인용)")]
    [Tooltip("에디터 Play에서만 적용. 온라인 랭킹 요청 구간에 최소 대기(초)를 넣어 로딩 UI를 볼 때 사용. 0이면 끔. 오프라인은 항상 즉시 표시. 빌드에서는 무시됩니다.")]
    [SerializeField, Min(0f)] float editorMinimumRankLoadingSeconds;

    private RankingScorePressable myScorePressable;
    private Coroutine scrollResetRoutine;
    private CanvasGroup contentLoadingCanvasGroup;
    private GameObject _runtimeLoadingOverlay;

    private void Awake()
    {
        if (rankingScrollRect == null)
        {
            rankingScrollRect = GetComponentInChildren<ScrollRect>(true);
        }

        ResolveMyRankReferences();
        EnsureMyScorePressable();
        HideStrayRankingItemPreview();
        EnsureRuntimeLoadingOverlay();
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

    public void SetScrollEnabled(bool enabled)
    {
        if (rankingScrollRect == null)
        {
            return;
        }

        rankingScrollRect.enabled = enabled;
        if (!enabled)
        {
            rankingScrollRect.velocity = Vector2.zero;
        }
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
        GameObject loadingRoot = loadingMessageObject != null ? loadingMessageObject : _runtimeLoadingOverlay;
        if (loadingRoot != null)
        {
            loadingRoot.SetActive(isLoading);
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

    /// <summary>
    /// 씬에 loadingMessageObject가 없을 때 Board 위에 기본 로딩 UI를 만듭니다.
    /// 나중에 디자인 프리팹을 연결하면 이 오버레이는 사용되지 않습니다.
    /// </summary>
    void EnsureRuntimeLoadingOverlay()
    {
        if (loadingMessageObject != null || _runtimeLoadingOverlay != null)
        {
            return;
        }

        Transform board = transform.Find("Window/Board");
        if (board == null)
        {
            return;
        }

        _runtimeLoadingOverlay = new GameObject("RankingLoadingOverlay", typeof(RectTransform), typeof(CanvasGroup));
        RectTransform rootRt = _runtimeLoadingOverlay.GetComponent<RectTransform>();
        rootRt.SetParent(board, false);
        StretchToFullParent(rootRt);
        rootRt.SetAsLastSibling();

        CanvasGroup rootCg = _runtimeLoadingOverlay.GetComponent<CanvasGroup>();
        rootCg.alpha = 1f;
        rootCg.blocksRaycasts = true;
        rootCg.interactable = false;

        GameObject dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        RectTransform dimRt = dimGo.GetComponent<RectTransform>();
        dimRt.SetParent(rootRt, false);
        StretchToFullParent(dimRt);
        Image dimImage = dimGo.GetComponent<Image>();
        Image boardBg = board.GetComponent<Image>();
        if (boardBg != null && boardBg.sprite != null)
        {
            dimImage.sprite = boardBg.sprite;
            dimImage.type = boardBg.type;
            dimImage.pixelsPerUnitMultiplier = boardBg.pixelsPerUnitMultiplier;
            dimImage.preserveAspect = boardBg.preserveAspect;
            dimImage.color = new Color(0f, 0f, 0f, 0.5f);
        }
        else
        {
            dimImage.sprite = GetOrCreateWhiteSprite();
            dimImage.type = Image.Type.Simple;
            dimImage.color = new Color(0f, 0f, 0f, 0.5f);
        }

        dimImage.raycastTarget = true;

        GameObject labelGo = new GameObject("LoadingLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.SetParent(rootRt, false);
        StretchToFullParent(labelRt);
        TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
        label.raycastTarget = false;
        label.text = "랭킹을 불러오는 중…";
        label.fontSize = 52;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.enableAutoSizing = false;
        if (myNameText != null)
        {
            label.font = myNameText.font;
            label.fontSharedMaterial = myNameText.fontSharedMaterial;
        }
        else if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }

        _runtimeLoadingOverlay.SetActive(false);
    }

    static void StretchToFullParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

#if UNITY_EDITOR
    /// <summary>에디터에서 랭킹 로딩 연출을 확인하기 위한 최소 대기(초).</summary>
    internal float GetEditorMinRankLoadingHoldSeconds()
    {
        return Mathf.Max(0f, editorMinimumRankLoadingSeconds);
    }
#else
    internal float GetEditorMinRankLoadingHoldSeconds() => 0f;
#endif

    static Sprite _cachedWhiteSprite;

    static Sprite GetOrCreateWhiteSprite()
    {
        if (_cachedWhiteSprite != null)
        {
            return _cachedWhiteSprite;
        }

        Texture2D tex = Texture2D.whiteTexture;
        _cachedWhiteSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100f);
        return _cachedWhiteSprite;
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
