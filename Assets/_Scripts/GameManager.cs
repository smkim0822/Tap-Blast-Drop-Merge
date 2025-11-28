using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static bool isRestart = false;

    public GameObject startPanel;       // 시작 화면
    public GameObject gameOverPanel;    // 게임 오버 화면

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 재시작인 경우 (Restart 버튼 누름)
        if (isRestart == true)
        {
            Time.timeScale = 1;
            startPanel.SetActive(false);
            gameOverPanel.SetActive(false);

            isRestart = false;
        }
        // 처음 들어온 경우
        else
        {
            Time.timeScale = 0;
            startPanel.SetActive(true);
            gameOverPanel.SetActive(false);
        }
    }

    // [게임 시작] 버튼용 함수
    public void OnClickStart()
    {
        startPanel.SetActive(false);
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
        gameOverPanel.SetActive(true);
    }
}