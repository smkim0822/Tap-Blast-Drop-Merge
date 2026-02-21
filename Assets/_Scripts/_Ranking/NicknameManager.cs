using UnityEngine;

public class NicknameManager : MonoBehaviour
{
    public static NicknameManager Instance;

    private const string PREFS_KEY = "PlayerName";

    private void Awake()
    {
        // 싱글톤 패턴 적용
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

    // [핵심 로직] 닉네임 초기화 및 자동 생성
    private void InitializeNickname()
    {
        // 1. 저장된 닉네임이 있는지 확인 (기본값은 빈 문자열)
        string currentName = PlayerPrefs.GetString(PREFS_KEY, "");

        // 2. 닉네임이 비어있다면 (최초 실행) -> Guest_XXXX 생성
        if (string.IsNullOrEmpty(currentName))
        {
            int randomNum = Random.Range(1000, 10000); // 1000 ~ 9999
            string newGuestName = $"Guest_{randomNum}";
            
            SetNickname(newGuestName);
            Debug.Log($"🆕 신규 유저 감지! 닉네임 자동 생성: {newGuestName}");
        }
        else
        {
            Debug.Log($"✅ 기존 유저 접속: {currentName}");
        }
    }

    // [기능] 닉네임 변경 및 저장
    public void SetNickname(string newName)
    {
        // 1. 빈 값 또는 공백 체크
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("⚠️ 닉네임은 비어있을 수 없습니다.");
            return;
        }

        // 2. 글자 수 제한 체크 (10자) - 초기 기획 반영
        if (newName.Length > 10)
        {
            Debug.LogWarning("⚠️ 닉네임은 10자를 초과할 수 없습니다.");
            // UI에 경고 팝업을 띄우는 로직을 추가할 수도 있습니다.
            return;
        }

        // 3. 저장
        PlayerPrefs.SetString(PREFS_KEY, newName);
        PlayerPrefs.Save(); 
        Debug.Log($"💾 닉네임 저장 완료: {newName}");
    }

    // [기능] 현재 닉네임 가져오기
    public string GetNickname()
    {
        return PlayerPrefs.GetString(PREFS_KEY, "Guest_Error");
    }
}