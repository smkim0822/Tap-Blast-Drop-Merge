using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources (오디오 소스 연결)")]
    [Tooltip("효과음을 재생할 오디오 소스 컴포넌트")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Settings (사운드 재생 설정)")]
    [Tooltip("동일한 효과음이 너무 짧은 간격으로 겹쳐 재생되는 것을 방지하는 쿨타임 (초)")]
    [SerializeField] private float minSoundInterval = 0.05f; // 같은 소리 반복 재생 방지 시간 (0.05초)
    [Tooltip("효과음 재생 시 피치를 미세하게 무작위 변경하여 기계적인 느낌을 줄일지 여부")]
    [SerializeField] private bool usePitchVariation = true;  // 피치 랜덤 사용 여부

    // 사운드 ON/OFF 상태 저장 변수
    private bool isSoundOn = true;

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

    /// <summary>
    /// 오브젝트 활성화 시 효과음 재생 이벤트 구독
    /// </summary>
    private void OnEnable()
    {
        AudioEvents.OnPlaySfx += PlaySFX;
        AudioEvents.OnRequestSetSoundMute += SetSoundMute;
    }

    /// <summary>
    /// 오브젝트 비활성화 시 효과음 재생 이벤트 해제
    /// </summary>
    private void OnDisable()
    {
        AudioEvents.OnPlaySfx -= PlaySFX;
        AudioEvents.OnRequestSetSoundMute -= SetSoundMute;
    }

    /// <summary>
    /// 기기에 저장된 효과음 ON/OFF 설정을 불러와 적용
    /// </summary>
    private void InitializeSettings()
    {
        isSoundOn = SoundSettingsStore.LoadIsSoundOn();
    }

    /// <summary>
    /// 효과음 재생
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="volume"></param>
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

    /// <summary>
    /// 외부에서 효과음 음소거 여부를 설정할 때 호출
    /// </summary>
    /// <param name="isMute"></param>
    public void SetSoundMute(bool isMute)
    {
        isSoundOn = !isMute;

        // 설정을 기기에 저장
        SoundSettingsStore.SaveIsSoundOn(isSoundOn);
    }
}