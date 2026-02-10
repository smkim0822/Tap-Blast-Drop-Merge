using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
    [Header("버튼 연결")]
    [SerializeField] private Button soundButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundButton;

    [Header("아이콘 이미지")]
    [SerializeField] private Image soundIconImage;
    [SerializeField] private Image musicIconImage;

    [Header("교체할 스프라이트 (On/Off)")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    [SerializeField] private Sprite musicOnSprite;
    [SerializeField] private Sprite musicOffSprite;

    private bool isSoundOn = true;
    private bool isMusicOn = true;

    private void Awake()
    {
        // soundButton.onClick.AddListener(OnClickSound);
        // musicButton.onClick.AddListener(OnClickMusic);
        // retryButton.onClick.AddListener(OnClickRetry);
        // mainButton.onClick.AddListener(OnClickMain);
    }

    private void OnEnable()
    {
        // 팝업 켜질 때 설정 불러오고 UI 갱신
        LoadSettings();
        UpdateUI();

        Time.timeScale = 0f; // 일시정지
    }

    private void OnDisable()
    {
        Time.timeScale = 1f; // 재개
    }

    // --- 버튼 기능 ---

    public void OnClickSound()
    {
        isSoundOn = !isSoundOn;
        // TODO: SoundManager.Instance.MuteSFX(!isSoundOn);
        SaveSettings();
        UpdateUI();
    }

    public void OnClickMusic()
    {
        isMusicOn = !isMusicOn;
        // TODO: SoundManager.Instance.MuteBGM(!isMusicOn);
        SaveSettings();
        UpdateUI();
    }

    public void OnClickRetry()
    {
        // 1. 팝업 닫기
        this.gameObject.SetActive(false);
        
        // 2. 게임 매니저에게 '재시작' 명령 (리셋 + 게임시작)
        GameManager.Instance.RetryGame();
    }

    public void OnClickMain()
    {
        // 1. 팝업 닫기
        this.gameObject.SetActive(false);

        // 2. 게임 매니저에게 '메인으로' 명령 (리셋 + 메인이동)
        GameManager.Instance.GoToMain();
    }

    public void OnClickClose()
    {
        this.gameObject.SetActive(false);
    }

    // --- 내부 로직 ---

    private void UpdateUI()
    {
        soundIconImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        musicIconImage.sprite = isMusicOn ? musicOnSprite : musicOffSprite;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
    }

    // 메인화면/인게임에 따라 버튼 보여주기
    public void SetMode(bool isIngame)
    {
        if (isIngame)
        {
            retryButton.gameObject.SetActive(true);
            mainButton.gameObject.SetActive(true);
        }
        else
        {
            retryButton.gameObject.SetActive(false);
            mainButton.gameObject.SetActive(false);
        }
    }
}