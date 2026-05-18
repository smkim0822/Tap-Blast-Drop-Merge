using UnityEngine;

/// <summary>
/// 닉네임의 로컬 저장/조회 책임을 캡슐화합니다.
/// </summary>
public static class NicknameStore
{
    public const string Key = "PlayerName";

    public static string Load(string fallback = "Guest_Error")
    {
        return PlayerPrefs.GetString(Key, fallback);
    }

    public static void Save(string nickname)
    {
        PlayerPrefs.SetString(Key, nickname);
        PlayerPrefs.Save();
    }
}
