using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 설정 팝업을 제어하는 스크립트.
/// 사운드 토글, 다시 하기, 메인으로 이동 및 팝업 애니메이션 기능 포함.
/// </summary>
public class SettingsPopup : MonoBehaviour
{
    [Header("Buttons (버튼 연결)")]
    [Tooltip("효과음 켜기/끄기 토글 버튼")]
    [SerializeField] private Button soundButton;
    // [SerializeField] private Button musicButton;
    [Tooltip("다시하기 버튼")]
    [SerializeField] private Button retryButton;
    [Tooltip("메인으로 버튼")]
    [SerializeField] private Button mainButton;
    [Tooltip("닫기 버튼")]
    [SerializeField] private Button closeButton;
    [Tooltip("배경 영역")]
    [SerializeField] private Button backgroundButton;

    [Header("Icons (아이콘)")]
    [Tooltip("효과음 ON/OFF 이미지")]
    [SerializeField] private Image soundIconImage;

    [Header("Sprites (On/Off) (상태별 스프라이트)")]
    [Tooltip("효과음 ON 이미지")]
    [SerializeField] private Sprite soundOnSprite;
    [Tooltip("효과음 OFF 이미지")]
    [SerializeField] private Sprite soundOffSprite;

    [Header("Animation Settings (팝업 연출 설정)")]
    [Tooltip("스케일 애니메이션이 적용될 팝업의 실제 부모 창")]
    [SerializeField] private RectTransform popupWindow;
    [Tooltip("파벙ㅂ이 나타날 때의 속도 변화를 나타내는 그래프")]
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("팝업이 열리고 닫히는 데 걸리는 시간 (초)")]
    [SerializeField] private float animDuration = 0.2f;

    /// <summary>효과음 ON/OFF 여부</summary>
    private bool isSoundOn = true;
    /// <summary>애니메이션 재생 여부</summary>
    private bool isAnimating = false;

    private void Start()
    {
        if (soundButton != null)
        {
            soundButton.onClick.AddListener(OnClickSound);
        }
        // if (musicButton != null)
        // {
        //     musicButton.onClick.AddListener(OnClickMusic);
        // }
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnClickRetry);
        }
        if (mainButton != null)
        {
            mainButton.onClick.AddListener(OnClickMain);
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnClickClose);
        }
        if (backgroundButton != null)
        {
            backgroundButton.onClick.AddListener(OnClickClose);
        }
    }

    private void OnDestory()
    {
        if (soundButton != null) soundButton.onClick.RemoveListener(OnClickSound);
        if (retryButton != null) retryButton.onClick.RemoveListener(OnClickRetry);
        if (mainButton != null) mainButton.onClick.RemoveListener(OnClickMain);
        if (closeButton != null) closeButton.onClick.RemoveListener(OnClickClose);
        if (backgroundButton != null) backgroundButton.onClick.RemoveListener(OnClickClose);
    }

    /// <summary>
    /// 설정 팝업을 열 때 호출.
    /// </summary>
    /// <param name="isIngame"></param>
    public void Open(bool isIngame)
    {
        if (isAnimating) return;

        gameObject.SetActive(true);
        SetMode(isIngame);
        LoadSettings();
        UpdateUI();

        StartCoroutine(AnimateOpen());
    }

    // --- 애니메이션 로직 ---
    
    IEnumerator AnimateOpen()
    {
        isAnimating = true;
        Time.timeScale = 0f;

        float timer = 0f;
        popupWindow.localScale = Vector3.zero;

        while (timer < animDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animDuration;
            float curveValue = openCurve.Evaluate(progress);

            popupWindow.localScale = new Vector3(curveValue, curveValue, 1f);

            yield return null;
        }

        popupWindow.localScale = Vector3.one;
        isAnimating = false;
    }

    IEnumerator AnimateClose()
    {
        isAnimating = true;

        float timer = 0f;
        Vector3 startScale = popupWindow.localScale;

        while (timer < animDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animDuration;

            // 점점 작아짐
            popupWindow.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        popupWindow.localScale = Vector3.zero;
        isAnimating = false;

        // 애니메이션 완료 후 게임 재개 및 비활성화
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    // --- 버튼 기능 ---

    public void OnClickSound()
    {
        isSoundOn = !isSoundOn;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSoundMute(!isSoundOn);
        }

        SaveSettings();
        UpdateUI();
    }

    // public void OnClickMusic()
    // {
    //     isMusicOn = !isMusicOn;

    //     if (SoundManager.Instance != null)
    //     {
    //         SoundManager.Instance.SetMusicMute(!isMusicOn);
    //     }
        
    //     SaveSettings();
    //     UpdateUI();
    // }

    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RetryGame();
        }
        gameObject.SetActive(false);
    }

    public void OnClickMain()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GoToMain();
        }
        gameObject.SetActive(false);
    }

    public void OnClickClose()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateClose());
    }

    // --- 내부 로직 ---

    private void UpdateUI()
    {
        if (soundIconImage != null)
        {
            soundIconImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
    }

    // 메인화면/인게임에 따라 버튼 보여주기
    public void SetMode(bool isIngame)
    {
        if (retryButton != null)
        {
            retryButton.gameObject.SetActive(isIngame);
        }
        if (mainButton != null)
        {
            mainButton.gameObject.SetActive(isIngame);
        }
    }
}
