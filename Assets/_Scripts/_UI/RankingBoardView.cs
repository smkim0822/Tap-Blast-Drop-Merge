using TMPro;
using UnityEngine;

/// <summary>
/// 랭킹 팝업의 내 순위/랭킹 리스트/오프라인 표시 렌더링을 담당합니다.
/// </summary>
[DisallowMultipleComponent]
public class RankingBoardView : MonoBehaviour
{
    [Header("UI References (기본 UI 연결)")]
    [SerializeField] Transform contentParent;
    [SerializeField] RankingItem itemPrefab;
    [SerializeField] GameObject offlineMessageObject;
    [SerializeField] GameObject loadingMessageObject;

    [Header("My Rank References (내 순위 UI)")]
    [SerializeField] TextMeshProUGUI myNameText;
    [SerializeField] TextMeshProUGUI myRankText;
    [SerializeField] TextMeshProUGUI myScoreText;

    public void ShowOffline(MyRankData myData, string displayName, int localBest)
    {
        SetLoading(false);
        ClearRankingItems();
        if (offlineMessageObject != null)
        {
            offlineMessageObject.SetActive(true);
        }

        UpdateMyRank(myData, displayName, localBest);
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
    }

    public void UpdateMyRank(MyRankData myData, string displayName, int localBest)
    {
        if (myNameText != null)
        {
            string finalName = (myData != null && !string.IsNullOrEmpty(myData.name)) ? myData.name : displayName;
            myNameText.text = finalName;
        }

        if (myRankText != null)
        {
            myRankText.text = (myData != null && myData.rank > 0) ? myData.rank.ToString() : "-";
        }

        int displayScore = myData != null ? Mathf.Max(myData.score, localBest) : localBest;
        if (myScoreText != null)
        {
            myScoreText.text = displayScore.ToString("N0");
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

        // 로딩 중에는 오프라인 메시지를 숨겨서 상태를 명확히 함
        if (isLoading && offlineMessageObject != null)
        {
            offlineMessageObject.SetActive(false);
        }
    }
}
