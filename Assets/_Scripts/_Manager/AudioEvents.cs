using System;
using UnityEngine;

/// <summary>
/// 오디오 재생/설정 관련 이벤트 모음입니다.
/// </summary>
public static class AudioEvents
{
    public static Action<AudioClip, float> OnPlaySfx;

    // 오디오 설정 요청 (true: 음소거)
    public static Action<bool> OnRequestSetSoundMute;
}

