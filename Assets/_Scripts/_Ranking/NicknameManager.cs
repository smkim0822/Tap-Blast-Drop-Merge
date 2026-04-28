using UnityEngine;
using System;

public class NicknameManager : MonoBehaviour
{
    public static NicknameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeNickname();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 게임 최초 실행 시 닉네임이 없다면 임의의 Guest 닉네임 생성.
    /// Awake 단계에서 자동 호출.
    /// </summary>
    private void InitializeNickname()
    {
        // 1. 저장된 닉네임이 있는지 확인 (기본값은 빈 문자열)
        string currentName = NicknameStore.Load("");

        // 2. 닉네임이 비어있다면 (최초 실행) -> Guest_XXXX 생성
        if (string.IsNullOrEmpty(currentName))
        {
            int randomNum = UnityEngine.Random.Range(1000, 10000); // 1000 ~ 9999
            string newGuestName = $"Guest_{randomNum}";
            
            SetNickname(newGuestName);
            Debug.Log($"🆕 신규 유저 감지! 닉네임 자동 생성: {newGuestName}");
        }
    }

    /// <summary>
    /// 새로운 닉네임을 검증하고 로컬 기기에 저장.
    /// </summary>
    /// <param name="newName">변경을 시도할 새 닉네임</param>
    /// <returns>저장 성공 여부 (true: 성공, false: 유효성 검사 실패)</returns>
    public bool SetNickname(string newName)
    {
        if (!NicknameValidator.IsValid(newName))
        {
            Debug.LogWarning($"⚠️ 닉네임은 비어있을 수 없으며 {NicknameValidator.MaxLength}자를 초과할 수 없습니다.");
            return false;
        }

        // 저장
        NicknameStore.Save(newName);

        return true;
    }

    /// <summary>
    /// 현재 기기에 저장된 플레이어의 닉네임을 불러옴.
    /// </summary>
    /// <returns>저장된 닉네임 (가져오기 실패 시 "Guest_Error" 반환)</returns>
    public string GetNickname()
    {
        return NicknameStore.Load("Guest_Error");
    }
}