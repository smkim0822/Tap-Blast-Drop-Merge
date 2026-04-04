using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Controllers (UI 컨트롤러)")]
    [Tooltip("메인 화면 패널")]
    public MainPanel mainPanel;
    [Tooltip("게임 오버 패널")]
    public GameOverPanel gameOverPanel;

    [Header("Game Objects (게임 오브젝트)")]
    [Tooltip("인게임 환경 그룹")]
    public GameObject gamePlayGroup;
    [Tooltip("인게임 UI")]
    public GameObject gamePlayUI;

    [Header("Player (플레이어)")]
    [Tooltip("플레이어 오브젝트")]
    public GameObject player;
    [Tooltip("플레이어 시작 위치")]
    public Transform playerStartPos;

    // 내부 상태 변수
    private bool canRetry = true;   // 재시작 버튼 활성화 여부

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 이벤트 구독: 게임 오버 이벤트 발생 시 TriggerGameOver를 실행하도록 등록
    /// </summary>
    private void OnEnable()
    {
        GameEvents.OnGameOver += TriggerGameOver;
    }

    /// <summary>
    /// 이벤트 해제: 오브젝트 파괴/비활성화 시 메모리 누수 방지를 위해 구독 해제
    /// </summary>
    private void OnDisable()
    {
        GameEvents.OnGameOver -= TriggerGameOver;
    }
    // ------

    void Start()
    {
        // 게임 시작 시 메인 화면 상태로 진입
        EnterMainState();

        // 게임 시작 시 서버와 최고 점수 동기화
        SyncBestScoreWithServer();
    }

    // --- 서버 데이터 동기화 로직 ---

    /// <summary>
    /// 서버로부터 현재 사용자의 최고 점수를 가져와 로컬 데이터와 동기화
    /// </summary>
    private void SyncBestScoreWithServer()
    {
        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.GetMyRank(OnSyncMyRankReceived));
        }
    }

    /// <summary>
    /// 서버 점수 수신 콜백: 서버 점수가 로컬 기록보다 높으면 로컬을 갱신
    /// </summary>
    /// <param name="success"></param>
    /// <param name="myData"></param>
    private void OnSyncMyRankReceived(bool success, MyRankData myData)
    {
        if (!success || myData == null) return;

        int localBestScore = PlayerPrefs.GetInt("BestScore", 0);
        int serverBestScore = myData.score;

        // 서버 점수가 로컬 점수보다 높을 경우에만 덮어쓰기
        if (serverBestScore > localBestScore)
        {
            PlayerPrefs.SetInt("BestScore", serverBestScore);
            PlayerPrefs.Save();
            Debug.Log($"[데이터 동기화] 로컬 점수 ({localBestScore})를 서버 최고 점수({serverBestScore})로 갱신했습니다.");

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ReloadBestScore();
            }
        }
    }

    // --- 상태 전환 로직 ---

    /// <summary>
    /// 게임 로비 상태로 전환
    /// </summary>
    public void EnterMainState()
    {
        Time.timeScale = 1f;

        if (mainPanel != null) mainPanel.gameObject.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.gameObject.SetActive(false);
        if (gamePlayGroup != null) gamePlayGroup.SetActive(false);
        if (gamePlayUI != null) gamePlayUI.SetActive(false);
    }

    /// <summary>
    /// 실제 게임 플레이 상태로 진입
    /// </summary>
    public void EnterPlayState()
    {
        Time.timeScale = 1f;

        if (mainPanel != null) mainPanel.gameObject.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.gameObject.SetActive(false);
        if (gamePlayGroup != null) gamePlayGroup.SetActive(true);
        if (gamePlayUI != null) gamePlayUI.SetActive(true);

        // 플레이어 리셋
        if (player != null)
        {
            player.SetActive(true);
            if (playerStartPos != null) player.transform.position = playerStartPos.position;

            Spawner s = player.GetComponent<Spawner>();
            if (s != null)
            {
                s.ResetSpawner();
            }

            GameEvents.OnGameStart?.Invoke();
        }
    }

    /// <summary>
    /// 오브젝트 초기화
    /// </summary>
    public void ResetData()
    {
        // 배치 ID(점수 배율) 초기화
        BombLogic.ResetBatchId();

        // 점수 초기화
        if (ScoreManager.Instance != null) 
        {
            ScoreManager.Instance.ResetScore();
        }
        
        // 맵 초기화
        GameObject[] stacks = GameObject.FindGameObjectsWithTag("Stack");
        if (stacks != null)
        {
            foreach (GameObject obj in stacks)
            {
                if (obj.GetComponent<GrainLogic>() != null)
                {
                    PoolManager.Instance.Despawn(obj);
                }
            }
        }

        // 폭탄 제거
        BombLogic[] fallingBombs = FindObjectsByType<BombLogic>(FindObjectsSortMode.None);
        foreach (BombLogic bomb in fallingBombs)
        {
            PoolManager.Instance.Despawn(bomb.gameObject);
        }

        // 이펙트 제거
        GameObject[] effects = GameObject.FindGameObjectsWithTag("Effect");
        if (effects != null)
        {
            foreach (GameObject effect in effects)
            {
                PoolManager.Instance.Despawn(effect);
            }
        }
    }

    /// <summary>
    /// [재시작] : 데이터 리셋 + 바로 게임 시작
    /// </summary>
    public void RetryGame()
    {
        if (!canRetry) return;

        ResetData();
        EnterPlayState();
    }

    /// <summary>
    /// [메인으로] : 데이터 리셋 + 메인 화면 이동
    /// </summary>
    public void GoToMain()
    {
        if (!canRetry) return;

        ResetData();
        EnterMainState();
    }

    /// <summary>
    /// 게임 오버
    /// </summary>
    public void TriggerGameOver()
    {
        Time.timeScale = 0; // 정지

        // 즉시 재시작 방지
        canRetry = false;
        StartCoroutine(EnableRetryRoutine());

        // 점수 가져오기
        int current = 0;
        if (ScoreManager.Instance != null)
        {
            current = ScoreManager.Instance.GetCurrentScore();
        }

        int best = PlayerPrefs.GetInt("BestScore", 0);
        bool isNewRecord = false;

        // 로컬 최고 기록 갱신
        if (current > best)
        {
            best = current;
            PlayerPrefs.SetInt("BestScore", best);
            PlayerPrefs.Save();
            isNewRecord = true;
        }

        // [서버 연동] 서버로 점수 전송
        if (RankingManager.Instance != null && current > 0)
        {
            StartCoroutine(RankingManager.Instance.PostScore(current));
        }

        // 인게임 UI 숨기기
        if (gamePlayUI != null)
        {
            gamePlayUI.SetActive(false);
        }

        // 결과창 표시 및 데이터 전달
        if (gameOverPanel != null)
        {
            gameOverPanel.gameObject.SetActive(true);
            gameOverPanel.Setup(current, best, isNewRecord);
        }
    }

    /// <summary>
    /// 실제 시간(Realtime)으로 1초 대기 후 버튼 활성화
    /// </summary>
    /// <returns></returns>
    IEnumerator EnableRetryRoutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        canRetry = true;
    }
}