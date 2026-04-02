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

        // 1~3등: 텍스트 숨기고 메달 이미지 활성화
        if (rank >= 1 && rank <= 3)
        {
            rankText.gameObject.SetActive(false);
            medalImage.gameObject.SetActive(true);

            if (rank == 1)
            {
                medalImage.sprite = goldMedal;
            }
            else if (rank == 2)
            {
                medalImage.sprite = silverMedal;
            }
            else if (rank == 3)
            {
                medalImage.sprite = bronzeMedal;
            }
        }
        // 4등 이하: 메달 숨기고 순위 텍스트 활성화
        else
        {
            medalImage.gameObject.SetActive(false);
            rankText.gameObject.SetActive(true);
            rankText.text = rank.ToString();
        }
    }
}