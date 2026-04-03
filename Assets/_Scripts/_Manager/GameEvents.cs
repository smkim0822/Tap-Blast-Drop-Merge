using UnityEngine;
using System;

// 게임 내 이벤트 관리
public class GameEvents : MonoBehaviour
{
    // 점수 획득 이벤트 (매개변수: 획득한 점수)
    public static Action<int> OnScoreAdded;

    // 효과음 재생 이벤트 (매개변수: 재생할 클립, 볼륨)
    public static Action<AudioClip, float> OnPlaySFX;

    // 게임 오버 이벤트
    public static Action OnGameOver;
}
