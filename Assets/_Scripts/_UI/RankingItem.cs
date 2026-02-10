using UnityEngine;
using TMPro;

public class RankingItem : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI scoreText;

    public void SetData(int rank, string name, int score)
    {
        rankText.text = rank.ToString();
        nameText.text = name;
        scoreText.text = score.ToString("N0");

        if (rank == 1) rankText.color = Color.yellow;
        else if (rank == 2) rankText.color = Color.gray;
        else if (rank == 3) rankText.color = new Color(0.8f, 0.5f, 0.2f);
        else rankText.color = Color.white;
    }
}