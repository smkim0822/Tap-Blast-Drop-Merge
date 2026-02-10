using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverPanel : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI resultLabel;   // Final Score / NEW RECORD
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI bestScoreText;

    [Header("Buttons")]
    public Button retryButton;
    // public Button mainbutton;

    [Header("Popups")]
    public RankingPopup rankingPopup;

    void Start()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        // if (mainButtom != null)
        // {
        //     mainButton.onClick.AddListener(OnMainClicked);
        // }
    }

    // 다시 하기
    void OnRetryClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryGame();
        }
    }

    // // 메인으로
    // void OnMainClicked()
    // {
    //     if (GameManager.Instance != null)
    //     {
    //         GameManager.Instance.GoToMain();
    //     }
    // }

    public void Setup(int currentScore, int bestScore, bool isNewRecord)
    {
        // 1. 점수 표기
        currentScoreText.text = currentScore.ToString("N0");
        bestScoreText.text = "Best: " + bestScore.ToString("N0");

        // 2. 타이틀 표기
        if (isNewRecord)
        {
            resultLabel.text = "<color=yellow>NEW RECORD!</color>";
        }
        else
        {
            resultLabel.text = "Final Score";
        }

        // 3. 켜기
        gameObject.SetActive(true);
    }

    public void OnClickRankingButton()
    {
        if (rankingPopup != null)
        {
            rankingPopup.OpenPopup();
        }
    }
}