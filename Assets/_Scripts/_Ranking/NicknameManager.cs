using UnityEngine;
using System;

public class NicknameManager : MonoBehaviour
{
    public static NicknameManager Instance;

    private const string PREFS_KEY = "PlayerName";

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

    // 닉네임 초기화 및 자동 생성
    private void InitializeNickname()
    {
        // 1. 저장된 닉네임이 있는지 확인 (기본값은 빈 문자열)
        string currentName = PlayerPrefs.GetString(PREFS_KEY, "");

        // 2. 닉네임이 비어있다면 (최초 실행) -> Guest_XXXX 생성
        if (string.IsNullOrEmpty(currentName))
        {
            int randomNum = UnityEngine.Random.Range(1000, 10000); // 1000 ~ 9999
            string newGuestName = $"Guest_{randomNum}";
            
            SetNickname(newGuestName);
            Debug.Log($"🆕 신규 유저 감지! 닉네임 자동 생성: {newGuestName}");
        }
    }

    // [기능] 닉네임 변경 및 저장
    public bool SetNickname(string newName)
    {
        // 빈 값 또는 공백 체크
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("⚠️ 닉네임은 비어있을 수 없습니다.");
            return false;
        }

        // 글자 수 제한 체크 (10자)
        if (newName.Length > 10)
        {
            Debug.LogWarning("⚠️ 닉네임은 10자를 초과할 수 없습니다.");
            return false;
        }

        // 저장
        PlayerPrefs.SetString(PREFS_KEY, newName);
        PlayerPrefs.Save(); 

        return true;
    }

    // [기능] 현재 닉네임 가져오기
    public string GetNickname()
    {
        return PlayerPrefs.GetString(PREFS_KEY, "Guest_Error");
    }
}