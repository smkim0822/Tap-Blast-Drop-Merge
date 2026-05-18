using System;

/// <summary>
/// 게임 세션(상태 전환, 게임오버, 재시작 등) 관련 이벤트 모음입니다.
/// </summary>
public static class SessionEvents
{
    public static Action OnGameOver;
    public static Action OnGameStart;

    // UI -> 세션 흐름 요청 (UI가 GameManager에 직접 의존하지 않도록 분리)
    public static Action OnRequestPlay;
    public static Action OnRequestRetry;
    public static Action OnRequestGoToMain;
}

