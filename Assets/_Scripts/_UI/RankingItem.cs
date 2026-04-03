using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RankingItem : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    [Header("Medal Settings")]
    public Image medalImage;
    public Sprite goldMedal;
    public Sprite silverMedal;
    public Sprite bronzeMedal;

    public void SetData(int rank, string name, int score)
    {
        nameText.text = name;
        scoreText.text = score.ToString("N0");

        bool isTop3 = rank >= 1 && rank <= 3;

        // 1~3등: 텍스트 숨기고 메달 이미지 활성화
        rankText.gameObject.SetActive(!isTop3);
        medalImage.gameObject.SetActive(isTop3);

        if (isTop3)
        {
            medalImage.sprite = rank == 1 ? goldMedal : (rank == 2 ? silverMedal : bronzeMedal);
        }
        else
        {
            rankText.text = rank.ToString();
        }
    }
}