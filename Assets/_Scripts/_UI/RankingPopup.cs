using UnityEngine;
using TMPro;
using UnityEngine.UI;
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

    [Header("Nickname Edit UI")]
    public Button editNameButton;       // 연필 모양 혹은 수정 버튼
    public GameObject editPopupPanel;   // 수정할 때 뜨는 작은 창(패널)
    public TMP_InputField nameInput;    // 새로운 이름을 입력할 곳
    public Button saveNameButton;       // 저장 버튼
    public Button cancelNameButton;     // 취소 버튼

    [Header("Animation Settings")]
    [SerializeField] private RectTransform popupWindow;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private float animDuration = 0.2f;

    private bool isAnimating = false;

    private void Start()
    {
        if (editNameButton != null)
            editNameButton.onClick.AddListener(OpenEditNamePopup);

        if (saveNameButton != null)
            saveNameButton.onClick.AddListener(SaveNewNickname);

        if (cancelNameButton != null)
            cancelNameButton.onClick.AddListener(CloseEditNamePopup);

        // 수정 팝업은 처음에 꺼둠
        if (editPopupPanel != null)
            editPopupPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (!isAnimating)
        {
            RefreshRanking();
        }
    }

    public void OpenPopup()
    {
        if (isAnimating) return;

        gameObject.SetActive(true);

        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.CheckServerStatus());
        }
        
        RefreshRanking();
        StartCoroutine(AnimateOpen());
    }

    public void ClosePopup()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateClose());
    }

    public void RefreshRanking()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        if (RankingManager.Instance != null)
        {
            StartCoroutine(RankingManager.Instance.GetRanks(OnDataReceived));
        }
    }

    void OnDataReceived(ScoreData[] ranks)
    {
        if (ranks == null || ranks.Length == 0) return;

        for (int i = 0; i < ranks.Length; i++)
        {
            int rank = i + 1;
            string playerName = ranks[i].name;
            int score = ranks[i].score;

            RankingItem newItem = Instantiate(itemPrefab, contentParent);
            newItem.SetData(rank, playerName, score);
        }

        UpdateMyRankUI();
    }

    // NicknameManager를 사용하여 실제 닉네임 표시
    void UpdateMyRankUI()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        
        string myName = NicknameManager.Instance != null 
                        ? NicknameManager.Instance.GetNickname() 
                        : "Guest";

        myNameText.text = myName;
        myScoreText.text = bestScore.ToString("N0");

        if (bestScore == 0) myRankText.text = "-";
        else myRankText.text = "?";
    }

    // --- 닉네임 수정 로직 ---

    private void OpenEditNamePopup()
    {
        if (editPopupPanel != null)
        {
            editPopupPanel.SetActive(true);
            // 입력창에 현재 닉네임을 미리 적어줌
            if (NicknameManager.Instance != null)
                nameInput.text = NicknameManager.Instance.GetNickname();
        }
    }

    private void CloseEditNamePopup()
    {
        if (editPopupPanel != null)
            editPopupPanel.SetActive(false);
    }

    private void SaveNewNickname()
    {
        if (nameInput == null) return;

        string newName = nameInput.text;

        // 매니저를 통해 저장 (매니저 내부에서 유효성 검사 수행함)
        if (NicknameManager.Instance != null)
        {
            NicknameManager.Instance.SetNickname(newName);
        }

        // UI 즉시 갱신 및 팝업 닫기
        UpdateMyRankUI();
        CloseEditNamePopup();
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