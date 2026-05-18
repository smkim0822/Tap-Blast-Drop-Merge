using UnityEngine;

/// <summary>
/// 랭킹 점수 표시용 포맷 (기본 축약 / 길게 누르기 시 전체 점수).
/// </summary>
public static class RankingScoreFormat
{
    private const int CompactThreshold = 100_000;

    public static bool UseCompactDisplay(int score)
    {
        return score >= CompactThreshold;
    }

    public static string FormatFull(int score)
    {
        if (score < 0)
        {
            score = 0;
        }

        return score.ToString("N0");
    }

    public static string FormatDisplay(int score)
    {
        if (score < 0)
        {
            score = 0;
        }

        if (!UseCompactDisplay(score))
        {
            return FormatFull(score);
        }

        if (score < 1_000_000)
        {
            return FormatCompactUnit(score, 1_000f, "K");
        }

        return FormatCompactUnit(score, 1_000_000f, "M");
    }

    private static string FormatCompactUnit(int score, float unit, string suffix)
    {
        float value = score / unit;
        if (value >= 100f)
        {
            return $"{Mathf.RoundToInt(value)}{suffix}";
        }

        float roundedTenth = Mathf.Round(value * 10f) / 10f;
        if (Mathf.Approximately(roundedTenth % 1f, 0f))
        {
            return $"{(int)roundedTenth}{suffix}";
        }

        return $"{roundedTenth:0.#}{suffix}";
    }
}
