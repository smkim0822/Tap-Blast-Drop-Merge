using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 랭킹 UI에서 한 명의 기록을 표시하는 아이템 스크립트.
/// 등수, 닉네임, 점수를 출력하며, 1~3등의 경우 숫자 대신 메달 이미지를 표시.
/// </summary>
public class RankingItem : MonoBehaviour
{
    [Header("UI Components (텍스트 연결)")]
    [Tooltip("등수를 표시할 텍스트")]
    public TextMeshProUGUI rankText;
    [Tooltip("닉네임을 표시할 텍스트")]
    public TextMeshProUGUI nameText;
    [Tooltip("점수를 표시할 텍스트")]
    public TextMeshProUGUI scoreText;

    [Header("Medal Settings (메달 이미지 설정)")]
    [Tooltip("1~3등일 때 등수 텍스트 대신 활성화될 메달 Image 컴포넌트")]
    public Image medalImage;
    [Tooltip("1등 메달 스프라이트")]
    public Sprite goldMedal;
    [Tooltip("2등 메달 스프라이트")]
    public Sprite silverMedal;
    [Tooltip("3등 메달 스프라이트")]
    public Sprite bronzeMedal;

    /// <summary>
    /// 서버나 로컬에서 받아온 랭킹 데이터를 UI에 세팅.
    /// </summary>
    /// <param name="rank">등수</param>
    /// <param name="name">닉네임</param>
    /// <param name="score">점수</param>
    public void SetData(int rank, string name, int score)
    {
        if (nameText != null) nameText.text = name;
        if (scoreText != null) scoreText.text = score.ToString("N0");

        // 1~3등 여부 판별
        bool isTop3 = rank >= 1 && rank <= 3;

        // 1~3등: 텍스트 숨기고 메달 이미지 활성화
        if (rankText != null) rankText.gameObject.SetActive(!isTop3);
        if (medalImage != null) medalImage.gameObject.SetActive(isTop3);

        if (isTop3)
        {
            if (medalImage != null)
            {
                medalImage.sprite = rank == 1 ? goldMedal : (rank == 2 ? silverMedal : bronzeMedal);
            }
        }
        else
        {
            if (rankText != null)
            {
                rankText.text = rank.ToString();
            }
        }
    }
}