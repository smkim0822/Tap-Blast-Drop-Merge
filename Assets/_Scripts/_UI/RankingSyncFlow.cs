using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 랭킹 팝업에서 서버 동기화와 오프라인 폴백 흐름을 담당합니다.
/// </summary>
public class RankingSyncFlow
{
    readonly MonoBehaviour coroutineHost;
    readonly RankingBoardView boardView;
    bool isSyncing = false;

    public RankingSyncFlow(MonoBehaviour host, RankingBoardView view)
    {
        coroutineHost = host;
        boardView = view;
    }

    public Coroutine RefreshRanking(Action onOfflineFallback = null)
    {
        if (isSyncing) return null;

        if (RankingManager.Instance == null || !RankingManager.Instance.IsNetworkReached())
        {
            RenderOfflineState();
            onOfflineFallback?.Invoke();
            return null;
        }

        boardView.SetLoading(true);
        float editorHold = boardView.GetEditorMinRankLoadingHoldSeconds();
        return coroutineHost.StartCoroutine(SyncAndLoadRankingRoutine(onOfflineFallback, editorHold));
    }

    public Coroutine SyncNicknameAndRefresh(string newName)
    {
        return coroutineHost.StartCoroutine(SyncNicknameAndRefreshRoutine(newName));
    }

    void RenderOfflineState()
    {
        boardView.SetLoading(false);
        string currentName = NicknameManager.Instance != null ? NicknameManager.Instance.GetNickname() : NicknameStore.Load("Guest");
        int localHighScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetBestScore() : BestScoreStore.Load();
        boardView.ShowOffline(null, $"{currentName} <size=70%><color=#FF7B7B>(Offline)</color></size>", localHighScore);
    }

    IEnumerator SyncAndLoadRankingRoutine(Action onOfflineFallback, float editorHoldSeconds)
    {
        isSyncing = true;

        if (editorHoldSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(editorHoldSeconds);
        }

        MyRankData serverMyData = null;
        bool isMyRankLoaded = false;
        bool isNetworkError = false;
        ScoreData[] loadedRanks = null;
        bool isRanksLoaded = false;

        coroutineHost.StartCoroutine(RankingManager.Instance.GetMyRank((success, data) =>
        {
            if (!success)
            {
                isNetworkError = true;
            }

            serverMyData = data;
            isMyRankLoaded = true;
        }));

        yield return new WaitUntil(() => isMyRankLoaded);

        if (isNetworkError)
        {
            RenderOfflineState();
            onOfflineFallback?.Invoke();
            isSyncing = false;
            yield break;
        }

        string localName = NicknameManager.Instance != null ? NicknameManager.Instance.GetNickname() : NicknameStore.Load("Guest");
        int localHighScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetBestScore() : BestScoreStore.Load();

        bool needsNicknameSync = serverMyData == null || localName != serverMyData.name;
        bool needsScoreSync = serverMyData == null ? (localHighScore > 0) : (localHighScore > serverMyData.score);

        if (needsNicknameSync)
        {
            yield return coroutineHost.StartCoroutine(RankingManager.Instance.UpdateNickname(localName));
        }

        if (needsScoreSync)
        {
            yield return coroutineHost.StartCoroutine(RankingManager.Instance.PostScore(localHighScore));
        }

        if (needsNicknameSync || needsScoreSync)
        {
            isMyRankLoaded = false;
            coroutineHost.StartCoroutine(RankingManager.Instance.GetMyRank((success, data) =>
            {
                if (!success)
                {
                    isNetworkError = true;
                }

                serverMyData = data;
                isMyRankLoaded = true;
            }));

            yield return new WaitUntil(() => isMyRankLoaded);

            if (isNetworkError)
            {
                RenderOfflineState();
                onOfflineFallback?.Invoke();
                isSyncing = false;
                yield break;
            }
        }

        coroutineHost.StartCoroutine(RankingManager.Instance.GetRanks((success, ranks) =>
        {
            if (!success)
            {
                isNetworkError = true;
            }

            loadedRanks = ranks;
            isRanksLoaded = true;
        }));

        yield return new WaitUntil(() => isRanksLoaded);

        if (isNetworkError)
        {
            RenderOfflineState();
            onOfflineFallback?.Invoke();
            isSyncing = false;
            yield break;
        }

        boardView.ShowOnline(serverMyData, localName, localHighScore, loadedRanks);
        isSyncing = false;
    }

    IEnumerator SyncNicknameAndRefreshRoutine(string newName)
    {
        if (RankingManager.Instance != null && !RankingManager.Instance.IsNetworkReached())
        {
            RenderOfflineState();
            yield break;
        }

        boardView.SetLoading(true);

        if (RankingManager.Instance != null)
        {
            yield return coroutineHost.StartCoroutine(RankingManager.Instance.UpdateNickname(newName));
        }

        RefreshRanking();
    }
}
