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
    public TextMeshProUGUI finalScoreText;  // 최종 점수 표시할 텍스트

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

    // [종료] 버튼용 함수
    public void OnClickQuit()
    {
        Debug.Log("게임 종료");
        Application.Quit();
    }

    // 게임 오버시 호출될 함수
    public void TriggerGameOver()
    {
        Time.timeScale = 0; // 시간 정지
        gameOverPanel.SetActive(true);  // 패널 켜기

        // 최종 점수 표시
        if (finalScoreText != null && ScoreManager.Instance != null)
        {
            int score = ScoreManager.Instance.GetCurrentScore();
            finalScoreText.text = "Final Score\n" + score.ToString();
        }
    }
}