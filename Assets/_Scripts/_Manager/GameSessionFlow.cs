using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 메인/플레이/게임오버 전환, 랭킹 동기화, 재시도 가드, 월드 리셋을 담당합니다.
/// UI 루트 토글은 <see cref="GameUIRoots"/>에 위임합니다.
/// </summary>
public class GameSessionFlow
{
    readonly MonoBehaviour _coroutineHost;
    readonly GameUIRoots _ui;

    bool _canRetry = true;

    public GameSessionFlow(MonoBehaviour coroutineHost, GameUIRoots ui)
    {
        _coroutineHost = coroutineHost;
        _ui = ui ?? throw new ArgumentNullException(nameof(ui));
    }

    public void SubscribeToGameOverEvent()
    {
        SessionEvents.OnGameOver += TriggerGameOver;
    }

    public void UnsubscribeFromGameOverEvent()
    {
        SessionEvents.OnGameOver -= TriggerGameOver;
    }

    public void SubscribeToUiRequests()
    {
        SessionEvents.OnRequestPlay += EnterPlayState;
        SessionEvents.OnRequestRetry += RetryGame;
        SessionEvents.OnRequestGoToMain += GoToMain;
    }

    public void UnsubscribeFromUiRequests()
    {
        SessionEvents.OnRequestPlay -= EnterPlayState;
        SessionEvents.OnRequestRetry -= RetryGame;
        SessionEvents.OnRequestGoToMain -= GoToMain;
    }

    public void Bootstrap()
    {
        EnterMainState();
        SyncBestScoreWithServer();
    }

    public void EnterMainState()
    {
        Time.timeScale = 1f;
        _ui.ShowMainMenuLayout();
    }

    public void EnterPlayState()
    {
        Time.timeScale = 1f;
        _ui.ShowGameplayLayout();
        _ui.PlacePlayerForNewRun();
    }

    public void ResetData()
    {
        GameplayWorldReset.ResetPooledContentAndScoring();
    }

    public void RetryGame()
    {
        if (!_canRetry) return;
        ResetData();
        EnterPlayState();
    }

    public void GoToMain()
    {
        if (!_canRetry) return;
        ResetData();
        EnterMainState();
    }

    public void TriggerGameOver()
    {
        Time.timeScale = 0f;
        _canRetry = false;
        _coroutineHost.StartCoroutine(EnableRetryAfterDelay());

        int current = ScoreManager.Instance != null ? ScoreManager.Instance.GetCurrentScore() : 0;
        int best = ScoreManager.Instance != null ? ScoreManager.Instance.GetBestScore() : BestScoreStore.Load();
        bool isNewRecord = current >= best && current > 0;

        if (RankingManager.Instance != null && current > 0)
        {
            _coroutineHost.StartCoroutine(RankingManager.Instance.PostScore(current));
        }

        _ui.ShowGameOverLayout(current, best, isNewRecord);
    }

    void SyncBestScoreWithServer()
    {
        if (RankingManager.Instance != null)
        {
            _coroutineHost.StartCoroutine(RankingManager.Instance.GetMyRank(OnSyncMyRankReceived));
        }
    }

    void OnSyncMyRankReceived(bool success, MyRankData myData)
    {
        if (!success || myData == null) return;

        int serverBestScore = myData.score;
        int localBestScore = ScoreManager.Instance != null ? ScoreManager.Instance.GetBestScore() : BestScoreStore.Load();

        if (serverBestScore > localBestScore)
        {
            Debug.Log($"[데이터 동기화] 로컬 점수 ({localBestScore})를 서버 최고 점수({serverBestScore})로 갱신했습니다.");

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.SyncBestScore(serverBestScore);
            }
            else
            {
                BestScoreStore.Save(serverBestScore);
            }
        }
    }

    IEnumerator EnableRetryAfterDelay()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        _canRetry = true;
    }
}
