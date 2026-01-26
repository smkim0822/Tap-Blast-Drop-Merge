using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Referernces")]
    public TextMeshProUGUI currentScoreText;    // 현재 점수 UI
    public TextMeshProUGUI bestScoreText;       // 최고 점수 UI

    private int currentScore = 0;
    private int bestScore = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 저장된 최고 점수 불러오기
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        // 최고 기록 경신 체크 및 저장
        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetInt("BestScore", bestScore);
            PlayerPrefs.Save();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentScoreText != null) currentScoreText.text = currentScore.ToString();

        if (bestScoreText != null) bestScoreText.text = "Best: " + bestScore.ToString();
    }

    // 외부에서 현재 점수를 가져갈 수 있게 하는 함수
    public int GetCurrentScore()
    {
        return currentScore;
    }
}
