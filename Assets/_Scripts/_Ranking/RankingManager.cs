using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System;

[System.Serializable]
public class ScoreData
{
    public string uuid;
    public string name;
    public int score;
    public string hash;
}

[System.Serializable]
public class MyRankData
{
    public int rank;
    public string name;
    public int score;
}

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;
    public ServerConfig config;

    public bool IsServerAlive { get; private set; } = false;
    private const string UUID_KEY = "PlayerUUID";

    private string BaseUrl
    {
        get
        {
// #if UNITY_EDITOR
//             return "http://localhost:8080/api";
// #else
            return $"http://{config.oracleIp}:8080/api";
// #endif
        }
    }

    private string SecretKey => config.secretKey;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(CheckServerStatus());
    }

    // UUID 획득 로직
    private string GetOrCreateUUID()
    {
        string uuid = PlayerPrefs.GetString(UUID_KEY, "");

        if (string.IsNullOrEmpty(uuid))
        {
            uuid = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(UUID_KEY, uuid);
            PlayerPrefs.Save();
            Debug.Log($"<color=cyan>✨ 새 UUID 생성 및 저장됨: {uuid}</color>");
        }

        return uuid;
    }

    // 점수 저장
    public IEnumerator PostScore(int score)
    {
        string playerName = PlayerPrefs.GetString("PlayerName", "Guest");
        string playerUUID = GetOrCreateUUID();
        string hash = GenerateHash(playerName, score, playerUUID);

        ScoreData data = new ScoreData();
        data.uuid = playerUUID;
        data.name = playerName;
        data.score = score;
        data.hash = hash;

        string json = JsonUtility.ToJson(data);

        UnityWebRequest request = new UnityWebRequest(BaseUrl + "/rankings", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ 점수 저장 성공!");
        }
        else
        {
            Debug.LogError("❌ 점수 저장 실패: " + request.downloadHandler.text);
        }
    }

    // Top 50 랭킹 조회
    public IEnumerator GetRanks(Action<ScoreData[]> onCallback)
    {
        UnityWebRequest request = UnityWebRequest.Get(BaseUrl + "/rankings");
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            ScoreData[] ranks = JsonHelper.FromJson<ScoreData>(jsonResult);
            if (onCallback != null) onCallback(ranks);
        }
        else
        {
            Debug.LogError($"❌ 랭킹 조회 실패! 에러: {request.error}");
            if (onCallback != null) onCallback(null);
        }
    }

    public IEnumerator GetMyRank(Action<MyRankData> onCallback)
    {
        string uuid = GetOrCreateUUID();
        UnityWebRequest request = UnityWebRequest.Get(BaseUrl + "/rankings/" + uuid);
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            MyRankData myRank = JsonUtility.FromJson<MyRankData>(jsonResult);
            if (onCallback != null) onCallback(myRank);
        }
        else
        {
            // 서버에서 데이터가 없으면 404 Not Found를 응답함
            if (request.responseCode == 404)
            {
                if (onCallback != null) onCallback(null); // 빈 데이터 처리
            }
            else
            {
                Debug.LogError($"❌ 내 랭킹 단건 조회 실패! 에러: {request.error}");
                if (onCallback != null) onCallback(null);
            }
        }
    }

    private string GenerateHash(string name, int score, string uuid)
    {
        string input = $"{name}:{score}:{uuid}:{SecretKey}";

        Debug.Log($"유니티 원본 문자열: [{input}]");

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashBytes) builder.Append(b.ToString("x2"));
            return builder.ToString();
        }
    }

    // 게임 시작 시 혹은 팝업을 열기 전 호출하여 서버 상태 갱신
    public IEnumerator CheckServerStatus()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(BaseUrl + "/health"))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();
            IsServerAlive = (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text == "ok");
        }
    }
}