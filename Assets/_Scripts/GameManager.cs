using UnityEngine;

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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        EnterMainState();
    }

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
    }

    // 데이터 초기화
    public void ResetData()
    {
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

        // 4. 플레이어 상태 리셋
        if (player != null)
        {
            Spawner spawnerScript = player.GetComponent<Spawner>();
            if (spawnerScript != null)
            {
                spawnerScript.ResetSpawner();
            }
            else
            {
                player.SetActive(true);
                if (playerStartPos != null)
                {
                    player.transform.position = playerStartPos.position;
                }
            }
        }
    }

    // 버튼 함수

    // [재시작] : 데이터 리셋 + 바로 게임 시작
    public void RetryGame()
    {
        ResetData();      // 점수, 위치 초기화
        EnterPlayState(); // 게임 시작
    }

    // [메인으로] : 데이터 리셋 + 메인 화면 이동
    public void GoToMain()
    {
        ResetData();      // 점수, 위치 초기화
        EnterMainState(); // 메인 화면 이동
    }

    // 게임 오버
    public void TriggerGameOver()
    {
        Time.timeScale = 0; // 정지

        // 점수 저장 및 갱신
        int current = ScoreManager.Instance.GetCurrentScore();
        int best = PlayerPrefs.GetInt("BestScore", 0);
        bool isNewRecord = false;

        if (current > best)
        {
            best = current;
            PlayerPrefs.SetInt("BestScore", best);
            PlayerPrefs.Save();
            isNewRecord = true;
        }

        if (gamePlayUI != null)
        {
            gamePlayUI.SetActive(false);   // 인게임 UI 숨김
        }
        if (gameOverPanel != null)
        {
            gameOverPanel.Setup(current, best, isNewRecord);    // 결과창 표시
        }
    }
}