using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class RankingPopup : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentParent;
    public RankingItem itemPrefab;

    [Header("My Rank References")]
    public TextMeshProUGUI myNameText;
    public TextMeshProUGUI myRankText;
    public TextMeshProUGUI myScoreText;

    [Header("Animation Settings")]
    [SerializeField] private RectTransform popupWindow;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private float animDuration = 0.2f;

    private bool isAnimating = false;

    private void OnEnable()
    {
        if (!isAnimating)
        {
            RefreshRanking();
        }
    }

    // 외부에서 팝업을 열 때 이 함수를 호출 (애니메이션 + 데이터 로드)

    public void OpenPopup()
    {
        if (isAnimating) return;

        gameObject.SetActive(true);
        RefreshRanking(); // 데이터 로드
        StartCoroutine(AnimateOpen());
    }

    public void ClosePopup()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateClose());
    }

    public void RefreshRanking()
    {
        // 1. 기존 리스트 초기화 (깨끗하게 비우기)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 서버에 요청 보내기 (비동기)
        // 싱글톤 인스턴스 확인 후 호출
        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.GetRanks(OnDataReceived));
        }
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

        if (bestScore == 0) myRankText.text = "-";
        else myRankText.text = "?"; // 전체 랭킹 밖이면 ?로 표시
    }

    // --- 애니메이션 로직 ---

    IEnumerator AnimateOpen()
    {
        isAnimating = true;

        float timer = 0f;
        popupWindow.localScale = Vector3.zero;

        while (timer < animDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animDuration;
            float curveValue = openCurve.Evaluate(progress);

            popupWindow.localScale = new Vector3(curveValue, curveValue, 1f);
            yield return null;
        }

        popupWindow.localScale = Vector3.one;
        isAnimating = false;
    }

    IEnumerator AnimateClose()
    {
        isAnimating = true;

        float timer = 0f;
        Vector3 startScale = popupWindow.localScale;

        while (timer < animDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animDuration;

            popupWindow.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
            yield return null;
        }

        popupWindow.localScale = Vector3.zero;
        isAnimating = false;

        gameObject.SetActive(false);
    }
}