using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [Header("Buttons (버튼 연결)")]
    public Button playButton;               // 플레이 버튼
    public Button settingButton;            // 설정 버튼
    public Button rankButton;               // 순위 버튼

    [Header("Popups (팝업 연결)")]
    public GameObject settingsPopup;        // 설정 팝업
    public RankingPopup rankingPopup;       // 순위 팝업

    void Start()
    {
        // 이벤트 리스너 연결
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        if (settingButton != null)
        {
            settingButton.onClick.AddListener(OnSettingsClicked);
        }
        if (rankButton != null)
        {
            rankButton.onClick.AddListener(OnRankingClicked);
        }
    }

    // ▶ 시작 버튼 클릭 시
    public void OnPlayClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnterPlayState();
        }
    }

    // ⚙️ 설정 버튼 클릭 시
    public void OnSettingsClicked()
    {
        if (settingsPopup != null)
        {
            SettingsPopup popupScript = settingsPopup.GetComponent<SettingsPopup>();
            if (popupScript != null)
            {
                popupScript.Open(false);
            }
        }
        else
        {
            Debug.LogError("MainPanel에 SettingsPopup이 연결되지 않았습니다.");
        }
    }

    // 🏆 순위 버튼
    public void OnRankingClicked()
    {
        if (rankingPopup != null)
        {
            rankingPopup.OpenPopup();
        }
    }
}