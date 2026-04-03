using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Controllers")]
    public MainPanel mainPanel;
    public GameOverPanel gameOverPanel;

    [Header("Game Objects")]
    public GameObject gamePlayGroup;    // 인게임 전체 그룹 (배경 등)
    public GameObject gamePlayUI;       // 인게임 UI

    [Header("Player")]
    public GameObject player;           // 플레이어 오브젝트
    public Transform playerStartPos;    // 플레이어 시작 위치

    // 재시작 딜레이를 위한 플래그
    private bool canRetry = true;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- 게임 오버 이벤트 구독 설정 ---
    private void OnEnable()
    {
        GameEvents.OnGameOver += TriggerGameOver;
    }

    private void OnDisable()
    {
        GameEvents.OnGameOver -= TriggerGameOver;
    }
    // ------

    void Start()
    {
        EnterMainState();

        // 게임 시작 시 서버와 최고 점수 동기화
        SyncBestScoreWithServer();
    }

    // --- 서버 데이터 동기화 로직 ---
    private void SyncBestScoreWithServer()
    {
        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.GetMyRank(OnSyncMyRankReceived));
        }
    }

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
    // ------

    // 상태 전환

    public void EnterMainState()
    {
        Time.timeScale = 1f;
        mainPanel.gameObject.SetActive(true);
        gameOverPanel.gameObject.SetActive(false);
        gamePlayGroup.SetActive(false);
        gamePlayUI.SetActive(false);
    }

    public void EnterPlayState()
    {
        Time.timeScale = 1f;
        mainPanel.gameObject.SetActive(false);
        gameOverPanel.gameObject.SetActive(false);
        gamePlayGroup.SetActive(true);
        gamePlayUI.SetActive(true);

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
        }
    }

    // 데이터 초기화 (재시작 및 메인 화면 이동 시 호출)
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

    // [재시작] : 데이터 리셋 + 바로 게임 시작
    public void RetryGame()
    {
        if (!canRetry) return;

        ResetData();
        EnterPlayState();
    }

    // [메인으로] : 데이터 리셋 + 메인 화면 이동
    public void GoToMain()
    {
        if (!canRetry) return;

        ResetData();
        EnterMainState();
    }

    // 게임 오버
    public void TriggerGameOver()
    {
        Time.timeScale = 0; // 정지

        // 즉시 재시작 방지 Lock
        canRetry = false;
        StartCoroutine(EnableRetryRoutine());

        // 점수 가져오기
        int current = ScoreManager.Instance.GetCurrentScore();
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
        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.PostScore(current));
        }

        if (gamePlayUI != null)
        {
            gamePlayUI.SetActive(false);
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.Setup(current, best, isNewRecord);    // 결과창 표시
        }
    }

    // 실제 시간(Realtime)으로 1초 대기 후 버튼 활성화
    IEnumerator EnableRetryRoutine()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        canRetry = true;
    }
}