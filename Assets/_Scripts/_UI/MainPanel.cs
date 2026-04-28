using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임의 메인 화면 UI를 제어하는 클래스.
/// 게임 시작, 랭킹 메뉴 진입을 담당.
/// </summary>
public class MainPanel : MonoBehaviour
{
    [Header("Buttons (버튼 연결)")]
    [Tooltip("게임을 시작하는 플레이 버튼")]
    public Button playButton;
    [Tooltip("설정 창을 여는 버튼")]
    public Button settingButton;
    [Tooltip("랭킹 창을 여는 버튼")]
    public Button rankButton;

    [Header("Popups (팝업 연결)")]
    [Tooltip("설정 버튼 클릭 시 활성화할 팝업 게임 오브젝트")]
    public GameObject settingsPopup;
    [Tooltip("순위 버튼 클릭 시 활성화할 팝업 게임 컴포넌트")]
    public RankingPopup rankingPopup;

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

    void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
        }
        if (settingButton != null)
        {
            settingButton.onClick.RemoveListener(OnSettingsClicked);
        }
        if (rankButton != null)
        {
            rankButton.onClick.RemoveListener(OnRankingClicked);
        }
    }

    /// <summary>
    /// 시작 버튼 클릭 시 호출.
    /// 플레이 상태로 진입.
    /// </summary>
    public void OnPlayClicked()
    {
        SessionEvents.OnRequestPlay?.Invoke();
    }

    /// <summary>
    /// 설정 버튼 클릭 시 호출.
    /// 설정 팝업.
    /// </summary>
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

    /// <summary>
    /// 순위 버튼 클릭 시 호출.
    /// 순위 팝업.
    /// </summary>
    public void OnRankingClicked()
    {
        if (rankingPopup != null)
        {
            rankingPopup.OpenPopup();
        }
    }
}