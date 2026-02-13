using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System; // Action을 쓰기 위해 필요

[System.Serializable]
public class ScoreData
{
    public string name;
    public int score;
    public string hash;
}

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance; // 어디서든 부를 수 있게 싱글톤 처리
    public bool IsServerAlive { get; private set; } = false;

    private const string BASE_URL = "http://localhost:8080/api";
    private const string SECRET_KEY = "UnityMergeGameSecretKey2026!"; 

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(CheckServerStatus());
    }

    // [기능 1] 점수 저장
    public IEnumerator PostScore(string playerName, int score)
    {
        string hash = GenerateHash(playerName, score);

        ScoreData data = new ScoreData();
        data.name = playerName;
        data.score = score;
        data.hash = hash;

        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(BASE_URL + "/scores", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ 점수 저장 성공!");
            // 저장이 끝나면 자동으로 랭킹 갱신 요청은 안 함 (UI가 알아서 할 것)
        }
        else
        {
            Debug.LogError("❌ 점수 저장 실패: " + request.downloadHandler.text);
        }
    }

    // [기능 2] 랭킹 조회 (UI에게 데이터를 넘겨주는 기능 추가)
    public IEnumerator GetRanks(Action<ScoreData[]> onCallback)
    {
        UnityWebRequest request = UnityWebRequest.Get(BASE_URL + "/ranks");
        request.timeout = 5;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            
            // JSON 데이터를 배열로 변환
            ScoreData[] ranks = JsonHelper.FromJson<ScoreData>(jsonResult);

            // UI에게 알림
            if (onCallback != null) onCallback(ranks);
        }
        else
        {
            Debug.LogError($"<color=yellow>❌ 랭킹 조회 실패!</color> 에러: {request.error} | 결과: {request.result}");
        }
    }

    private string GenerateHash(string name, int score)
    {
        string input = name + score + SECRET_KEY;
        SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = sha256.ComputeHash(bytes);
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++) builder.Append(hashBytes[i].ToString("x2"));
        return builder.ToString();
    }

    // 게임 시작 시 혹은 팝업을 열기 전 호출하여 서버 상태 갱신
    public IEnumerator CheckServerStatus()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(BASE_URL + "/health"))
        {
            // 서버 응답 대기 시간 설정
            request.timeout = 3;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text == "ok")
            {
                IsServerAlive = true;
                Debug.Log("<color=green>🌐 서버 연결 성공: 상태 정상</color>");
            }
            else
            {
                IsServerAlive = false;
                Debug.LogWarning("<color=red>🌐 서버 연결 실패: 서버가 꺼져 있거나 주소가 잘못되었습니다.</color>");
            }
        }
    }
}