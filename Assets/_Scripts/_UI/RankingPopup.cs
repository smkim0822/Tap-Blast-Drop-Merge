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

    private void OnEnable()
    {
        // 팝업이 켜질 때마다 서버에서 데이터를 가져옴
        RefreshRanking();
    }

    public void RefreshRanking()
    {
        // 1. 기존 리스트 초기화 (깨끗하게 비우기)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 서버에 요청 보내기 (비동기)
        // "데이터 다 가져오면 OnDataReceived 함수를 실행해줘!" 라고 부탁함
        StartCoroutine(RankingManager.Instance.GetRanks(OnDataReceived));
    }

    // 서버에서 데이터가 도착하면 실행되는 함수
    void OnDataReceived(ScoreData[] ranks)
    {
        // 데이터가 없으면 리턴
        if (ranks == null || ranks.Length == 0) return;

        // 3. 받아온 데이터로 리스트 만들기
        for (int i = 0; i < ranks.Length; i++)
        {
            int rank = i + 1;
            string playerName = ranks[i].name;
            int score = ranks[i].score;

            RankingItem newItem = Instantiate(itemPrefab, contentParent);
            newItem.SetData(rank, playerName, score);
        }

        // 4. 내 순위 정보 업데이트 (로컬 정보 기반)
        UpdateMyRankUI();
    }

    void UpdateMyRankUI()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        string myName = "Me"; // 나중에 닉네임 입력 기능 넣으면 PlayerPrefs로 변경

        myNameText.text = myName;
        myScoreText.text = bestScore.ToString("N0");

        // (심화) 만약 서버 랭킹에 내가 있다면 내 등수를 표시해줄 수도 있음
        // 지금은 일단 간단하게 처리
        if (bestScore == 0) myRankText.text = "-";
        else myRankText.text = "?"; // 전체 랭킹 밖이면 ?로 표시
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);
        // OnEnable이 자동으로 호출되어 랭킹을 갱신함
    }

    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }
}