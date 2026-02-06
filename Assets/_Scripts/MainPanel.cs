using UnityEngine;
using UnityEngine.UI;

public class MainPanel : MonoBehaviour
{
    [Header("Buttons (버튼 연결)")]
    public Button playButton;       // 플레이 버튼
    public Button settingButton;    // 설정 버튼
    public Button rankButton;       // 순위 버튼

    [Header("Popups (팝업 연결)")]
    public GameObject settingsPopup;     // 설정 팝업
    public GameObject rankingPopup;     // 순위 팝업

    void Start()
    {
        // 이벤트 리스너 연결
        // playButton.onClick.AddListener(OnPlayClicked);
        // settingButton.onClick.AddListener(OnSettingsClicked);
        // rankButton.onClick.AddListener(OnRankClicked);
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
            // 1. 팝업 활성화
            settingsPopup.SetActive(true);

            // 2. 메인 화면 모드로 설정
            SettingsPopup popupScript = settingsPopup.GetComponent<SettingsPopup>();
            if (popupScript != null)
            {
                popupScript.SetMode(false);
            }
        }
        else
        {
            Debug.LogError("MainPanel에 SettingsPopup이 연결되지 않았습니다.");
        }
    }

    // 🏆 순위 버튼
    public void OnRankClicked()
    {
        if (rankingPopup != null)
        {
            rankingPopup.SetActive(true);
        }
    }
}