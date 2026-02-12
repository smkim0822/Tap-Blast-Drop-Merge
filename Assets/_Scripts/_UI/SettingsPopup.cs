using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Animation Settings")]
    [SerializeField] private RectTransform popupWindow;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private float animDuration = 0.2f;

    private bool isSoundOn = true;
    private bool isMusicOn = true;
    private bool isAnimating = false;

    // 팝업을 열 때 호출 (외부에서 사용)
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

    public void OnClickMusic()
    {
        isMusicOn = !isMusicOn;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicMute(!isMusicOn);
        }
        
        SaveSettings();
        UpdateUI();
    }

    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        GameManager.Instance.RetryGame();
        gameObject.SetActive(false);
    }

    public void OnClickMain()
    {
        Time.timeScale = 1f;
        GameManager.Instance.GoToMain();
        gameObject.SetActive(false);
    }

    public void OnClickClose()
    {
        if (isAnimating)
        {
            return;
        }
        StartCoroutine(AnimateClose());
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