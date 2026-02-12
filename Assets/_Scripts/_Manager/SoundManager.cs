using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;     // 배경음
    [SerializeField] private AudioSource sfxSource;     // 효과음

    private bool isSoundOn = true;
    private bool isMusicOn = true;

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeSettings();
    }

    private void InitializeSettings()
    {
        // 저장된 설정 불러오기
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;

        // 배경음 초기 상태 적용
        if (bgmSource != null)
        {
            bgmSource.mute = !isMusicOn;
            if (!bgmSource.isPlaying && isMusicOn)
            {
                bgmSource.Play();
            }
        }
    }

    // --- 외부 호출용 메서드 ---

    // 효과음 재생 (GrainLogic에서 호출)
    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (!isSoundOn || clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip, volume);
    }

    // 설정 팝업에서 호출: 효과음 ON/OFF 토글
    public void SetSoundMute(bool isMute)
    {
        isSoundOn = !isMute;
    }

    // 설정 팝업에서 호출: 배경음 ON/OFF 토글
    public void SetMusicMute(bool isMute)
    {
        isMusicOn = !isMute;
        if (bgmSource != null)
        {
            bgmSource.mute = isMute;
        }
    }

}
