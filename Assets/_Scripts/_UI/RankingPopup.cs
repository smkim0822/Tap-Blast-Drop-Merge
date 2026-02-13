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

    // 테스트용 더미 데이터
    private void OnEnable()
    {
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
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        myNameText.text = "Me";
        myScoreText.text = bestScore.ToString("N0");

        if (bestScore == 0) myRankText.text = "-";
        else myRankText.text = "999+";
    }

    // --- 애니메이션 로직 (SettingsPopup에서 이식) ---

    public void OpenPopup()
    {
        if (isAnimating) return;

        gameObject.SetActive(true);
        RefreshRanking(); // 데이터 먼저 세팅
        StartCoroutine(AnimateOpen());
    }

    public void ClosePopup()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateClose());
    }

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