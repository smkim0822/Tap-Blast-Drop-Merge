using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static bool isRestart = false;

    [Header("Panels")]
    public GameObject startPanel;       // 시작 화면
    public GameObject gameOverPanel;    // 게임 오버 화면
    public GameObject pausePanel;       // 일시정지 화면

    [Header("Game Groups")]
    public GameObject gamePlayObjects;  // 월드 오브젝트
    public GameObject gamePlayUI;       // 인게임 UI

    [Header("Game Over UI")]
    public TextMeshProUGUI resultLabelText;     // 'Final Score' 또는 'New Record!'
    public TextMeshProUGUI currentScoreText;    // 이번 판 점수
    public TextMeshProUGUI bestScoreText;       // 최고 기록

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        // [재시작] 모드로 온 경우
        if (isRestart == true)
        {
            Time.timeScale = 1;
            startPanel.SetActive(false);
            gameOverPanel.SetActive(false);

            if (gamePlayObjects != null) gamePlayObjects.SetActive(true);
            if (gamePlayUI != null) gamePlayUI.SetActive(true);

            isRestart = false;
        }
        // [메인 메뉴] 모드로 온 경우
        else
        {
            Time.timeScale = 1;
            startPanel.SetActive(true);
            gameOverPanel.SetActive(false);

            if (gamePlayObjects != null) gamePlayObjects.SetActive(false);
            if (gamePlayUI != null) gamePlayUI.SetActive(false);
        }
    }

    // [일시정지] 버튼 클릭
    public void OnClickPause()
    {
        Time.timeScale = 0;
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    // [계속 하기] 버튼 클릭
    public void OnClickResume()
    {
        Time.timeScale = 1;
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    // [메인으로] 버튼 클릭
    public void OnClickGoToMain()
    {
        Time.timeScale = 1;
        isRestart = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // [게임 시작] 버튼용 함수
    public void OnClickStart()
    {
        startPanel.SetActive(false);

        if (gamePlayObjects != null) gamePlayObjects.SetActive(true);
        if (gamePlayUI != null) gamePlayUI.SetActive(true);

        Time.timeScale = 1; // 시간 흐르게 하기
    }

    // [다시 하기] 버튼용 함수
    public void OnClickRestart()
    {
        // 시작 메뉴 안켜지게
        isRestart = true;
        
        // 현재 씬을 다시 로드 (초기화)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 게임 오버시 호출될 함수
    public void TriggerGameOver()
    {
        Time.timeScale = 0;     // 시간 정지
        
        // 1. 점수 처리 로직
        int currentScore = 0;
        if (ScoreManager.Instance != null)
        {
            currentScore = ScoreManager.Instance.GetCurrentScore();
        }

        // 저장된 최고 점수 불러오기 (없으면 0)
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        // 2. 신기록 판정
        if (currentScore > bestScore)
        {
            // 신기록 갱신!
            bestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();

            // 텍스트 변경: 신기록
            if (resultLabelText != null)
            {
                resultLabelText.text = "<color=yellow>NEW RECORD!</color>";
            }
        }
        else
        {
            // 기록 갱신 실패
            if (resultLabelText != null)
            {
                resultLabelText.text = "Final Score";
            }
        }

        // 3. 점수 텍스트 표시
        if (currentScoreText != null)
        {
            currentScoreText.text = currentScore.ToString("N0");
        }
        if (bestScoreText != null)
        {
            bestScoreText.text = bestScore.ToString("N0");
        }

        gameOverPanel.SetActive(true);
    }
}