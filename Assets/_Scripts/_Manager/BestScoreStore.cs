using UnityEngine;

/// <summary>
/// 최고 점수의 영속화와 조회를 한 곳에서 관리합니다.
/// PlayerPrefs 키를 프로젝트 전반에서 중복 사용하지 않도록 캡슐화합니다.
/// </summary>
public static class BestScoreStore
{
    public const string Key = "BestScore";

    public static int Load()
    {
        return PlayerPrefs.GetInt(Key, 0);
    }

    public static bool TrySaveIfHigher(int score)
    {
        if (score <= Load())
        {
            return false;
        }

        Save(score);
        return true;
    }

    public static void Save(int score)
    {
        PlayerPrefs.SetInt(Key, score);
        PlayerPrefs.Save();
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(Key);
    }
}
