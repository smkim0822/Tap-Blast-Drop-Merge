using TMPro;
using UnityEngine;

/// <summary>
/// 랭킹 UI에서 한 명의 기록을 표시합니다.
/// 1~3등은 등수 색상만 강조하고, 4등 이하는 기본 색으로 표시합니다.
/// </summary>
public class RankingItem : MonoBehaviour
{
    private const int MaxNicknameLength = 10;

    [Header("UI Components")]
    [Tooltip("등수를 표시할 텍스트")]
    public TextMeshProUGUI rankText;
    [Tooltip("닉네임을 표시할 텍스트")]
    public TextMeshProUGUI nameText;
    [Tooltip("점수를 표시할 텍스트")]
    public TextMeshProUGUI scoreText;

    public void SetData(int rank, string name, int score)
    {
        Color rowColor = RankingUiTheme.GetRankTextColor(rank);
        bool isTop3 = rank >= 1 && rank <= 3;
        string displayName = TruncateNickname(name);

        if (nameText != null)
        {
            nameText.text = displayName;
            nameText.color = rowColor;
            ApplyNameTextStyle(nameText, isTop3);
            DisableRaycast(nameText);
        }

        if (scoreText != null)
        {
            scoreText.color = rowColor;
            scoreText.fontStyle = isTop3 ? FontStyles.Bold : FontStyles.Normal;
            DisableRaycast(scoreText);

            RankingScorePressable scorePressable = GetOrAddRowScorePressable();
            scorePressable.Bind(scoreText);
            scorePressable.SetScore(score);
        }

        if (rankText != null)
        {
            rankText.gameObject.SetActive(true);
            rankText.text = rank.ToString();
            rankText.color = rowColor;
            ApplyRowTextStyle(rankText, isTop3);
            DisableRaycast(rankText);
        }
    }

    private RankingScorePressable GetOrAddRowScorePressable()
    {
        RankingScorePressable pressable = GetComponent<RankingScorePressable>();
        if (pressable == null)
        {
            pressable = gameObject.AddComponent<RankingScorePressable>();
        }

        RemoveStalePressableOnScoreText(pressable);
        return pressable;
    }

    private void RemoveStalePressableOnScoreText(RankingScorePressable rowPressable)
    {
        if (scoreText == null)
        {
            return;
        }

        RankingScorePressable onScoreText = scoreText.GetComponent<RankingScorePressable>();
        if (onScoreText != null && onScoreText != rowPressable)
        {
            Destroy(onScoreText);
        }
    }

    private static string TruncateNickname(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return string.Empty;
        }

        return name.Length <= MaxNicknameLength ? name : name.Substring(0, MaxNicknameLength);
    }

    private static void ApplyRowTextStyle(TextMeshProUGUI text, bool isTop3)
    {
        text.fontStyle = isTop3 ? FontStyles.Bold : FontStyles.Normal;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private static void ApplyNameTextStyle(TextMeshProUGUI text, bool isTop3)
    {
        text.fontStyle = isTop3 ? FontStyles.Bold : FontStyles.Normal;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
    }

    private static void DisableRaycast(TextMeshProUGUI text)
    {
        text.raycastTarget = false;
    }
}
