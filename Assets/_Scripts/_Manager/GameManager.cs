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

    // 데이터 초기화
    public void ResetData()
    {
        // 0. 배치 ID(점수 배율) 초기화
        BombLogic.ResetBatchId();

        // 1. 점수 초기화
        if (ScoreManager.Instance != null) 
        {
            ScoreManager.Instance.ResetScore();
        }
        
        // 2. 맵 초기화
        GameObject[] stacks = GameObject.FindGameObjectsWithTag("Stack");
        if (stacks != null)
        {
            foreach (GameObject obj in stacks)
            {
                if (obj.GetComponent<GrainLogic>() != null)
                {
                    Destroy(obj);
                }
            }
        }

        // 3. 폭탄 제거
        BombLogic[] fallingBombs = FindObjectsByType<BombLogic>(FindObjectsSortMode.None);
        foreach (BombLogic bomb in fallingBombs)
        {
            Destroy(bomb.gameObject);
        }

        // 4. 이펙트 제거
        GameObject[] effects = GameObject.FindGameObjectsWithTag("Effect");
        if (effects != null)
        {
            foreach (GameObject effect in effects)
            {
                Destroy(effect);
            }
        }
    }

    // 버튼 함수

    // [재시작] : 데이터 리셋 + 바로 게임 시작
    public void RetryGame()
    {
        // 딜레이 중이면 클릭 무시
        if (!canRetry) return;

        ResetData();      // 점수, 위치 초기화
        EnterPlayState(); // 게임 시작
    }

    // [메인으로] : 데이터 리셋 + 메인 화면 이동
    public void GoToMain()
    {
        if (!canRetry) return; // 메인으로 가는 것도 게임오버 직후 실수 방지

        ResetData();      // 점수, 위치 초기화
        EnterMainState(); // 메인 화면 이동
    }

    // 게임 오버 (여기가 핵심!)
    public void TriggerGameOver()
    {
        Time.timeScale = 0; // 정지

        // 즉시 재시작 방지 Lock
        canRetry = false;
        // Invoke 대신 Realtime 코루틴 시작
        StartCoroutine(EnableRetryRoutine());

        // 점수 가져오기
        int current = ScoreManager.Instance.GetCurrentScore();
        int best = PlayerPrefs.GetInt("BestScore", 0);
        bool isNewRecord = false;

        // 로컬 최고 기록 갱신 (내 폰에 저장)
        if (current > best)
        {
            best = current;
            PlayerPrefs.SetInt("BestScore", best);
            PlayerPrefs.Save();
            isNewRecord = true;
        }

        // ==========================================
        // 🔥 [서버 연동 추가] 서버로 점수 전송 🔥
        // ==========================================
        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.PostScore(current));
        }
        // ==========================================

        if (gamePlayUI != null)
        {
            gamePlayUI.SetActive(false);   // 인게임 UI 숨김
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