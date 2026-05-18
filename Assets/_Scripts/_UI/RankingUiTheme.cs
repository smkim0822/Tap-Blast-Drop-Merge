using UnityEngine;

/// <summary>
/// 랭킹 팝업 피그마 스펙 색상 (#2d3269, #f7e07c, #e0e0e0, #c9823b).
/// </summary>
public static class RankingUiTheme
{
    public static readonly Color PrimaryText = new Color(45f / 255f, 50f / 255f, 105f / 255f, 1f);
    public static readonly Color GoldText = new Color(247f / 255f, 224f / 255f, 124f / 255f, 1f);
    public static readonly Color SilverText = new Color(224f / 255f, 224f / 255f, 224f / 255f, 1f);
    public static readonly Color BronzeText = new Color(201f / 255f, 130f / 255f, 59f / 255f, 1f);

    public static Color GetRankTextColor(int rank)
    {
        switch (rank)
        {
            case 1: return GoldText;
            case 2: return SilverText;
            case 3: return BronzeText;
            default: return PrimaryText;
        }
    }
}
