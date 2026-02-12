using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RankingPopup : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public RankingItem itemPrefab;

    [Header("My Rank References")]
    public TextMeshProUGUI myNameText;
    public TextMeshProUGUI myRankText;
    public TextMeshProUGUI myScoreText;

    // 테스트용 더미 데이터
    private void OnEnable()
    {
        RefreshRanking();
    }

    public void RefreshRanking()
    {
        // 1. 기존 리스트 초기화
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 더미 데이터 20개 생성
        for (int i = 0; i < 20; i++)
        {
            int rank = i + 1;
            int score = 50000 - (i * 1234);
            string playerName = $"Player {Random.Range(100, 999)}";

            RankingItem newItem = Instantiate(itemPrefab, contentParent);
            newItem.SetData(rank, playerName, score);
        }

        // 3. 내 순위 정보 업데이트
        UpdateMyRankUI();
    }

    void UpdateMyRankUI()
    {
        // 저장된 최고 점수 가져오기
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        myNameText.text = "Me";
        myScoreText.text = bestScore.ToString("N0");

        // 순위 임의 처리
        if (bestScore == 0)
        {
            myRankText.text = "-";
        }
        else
        {
            myRankText.text = "999+";
        }
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);
        RefreshRanking();
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}