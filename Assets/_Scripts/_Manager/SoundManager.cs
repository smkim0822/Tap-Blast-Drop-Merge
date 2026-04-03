using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings")]
    [SerializeField] private float minSoundInterval = 0.05f; // 같은 소리 반복 재생 방지 시간 (0.05초)
    [SerializeField] private bool usePitchVariation = true;  // 피치 랜덤 사용 여부

    private bool isSoundOn = true;
    private bool isMusicOn = true;

    // 마지막으로 소리가 재생된 시간을 기록 (Key: 오디오클립, Value: 시간)
    private Dictionary<AudioClip, float> lastPlayedTimes = new Dictionary<AudioClip, float>();

    void Awake()
    {
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

    // --- 사운드 재생 이벤트 구독 설정 ---
    private void OnEnable()
    {
        GameEvents.OnPlaySFX += PlaySFX;
    }

    private void OnDisable()
    {
        GameEvents.OnPlaySFX -= PlaySFX;
    }
    // ------

    private void InitializeSettings()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;

        if (bgmSource != null)
        {
            bgmSource.mute = !isMusicOn;
            if (!bgmSource.isPlaying && isMusicOn)
            {
                bgmSource.Play();
            }
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1.0f)
    {
        if (!isSoundOn || clip == null || sfxSource == null) return;

        // 쿨타임 체크
        if (lastPlayedTimes.ContainsKey(clip))
        {
            float lastTime = lastPlayedTimes[clip];
            if (Time.time - lastTime < minSoundInterval)
            {
                return;
            }
        }

        // 재생 시간 갱신
        lastPlayedTimes[clip] = Time.time;

        // 피치 랜덤 변화 (기계음 방지)
        if (usePitchVariation)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
        }
        else
        {
            sfxSource.pitch = 1.0f;
        }

        // 재생
        sfxSource.PlayOneShot(clip, volume);
    }

    public void SetSoundMute(bool isMute)
    {
        isSoundOn = !isMute;
    }

    public void SetMusicMute(bool isMute)
    {
        isMusicOn = !isMute;
        if (bgmSource != null)
        {
            bgmSource.mute = isMute;
        }
    }
}