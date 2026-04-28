using UnityEngine;

/// <summary>
/// 효과음 ON/OFF 설정의 로컬 저장/조회 책임을 캡슐화합니다.
/// </summary>
public static class SoundSettingsStore
{
    public const string Key = "SoundOn";

    public static bool LoadIsSoundOn()
    {
        return PlayerPrefs.GetInt(Key, 1) == 1;
    }

    public static void SaveIsSoundOn(bool isSoundOn)
    {
        PlayerPrefs.SetInt(Key, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
