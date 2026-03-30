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

[System.Serializable]
public class NicknameUpdateReq
{
    public string name;
}

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;

    private ServerConfig config;
    private bool isConfigMissing = false;

    public bool IsServerAlive { get; private set; } = false;
    private const string UUID_KEY = "PlayerUUID";

    private string BaseUrl
    {
        get
        {
            if (isConfigMissing || config == null){
                return "http://localhost:8080/api";
            }

            return $"https://{config.oracleIp}/api";
        }
    }

    private string SecretKey
    {
        get
        {
            if (isConfigMissing || config == null) return "dummy_secret_key";
            return config.secretKey;
        }
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadServerConfig();
    }

    void Start()
    {
        StartCoroutine(CheckServerStatus());
    }

    private void LoadServerConfig(){
        config = Resources.Load<ServerConfig>("ServerConfig");

        if (config == null){
            isConfigMissing = true;
            Debug.LogWarning("⚠️ [RankingManager] Resources/ServerConfig.asset 파일을 찾을 수 없습니다! 로컬 더미 설정으로 실행됩니다.");
        }
        else {
            isConfigMissing = false;
        }
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
        if (isConfigMissing){
            Debug.LogWarning("⚠️ [RankingManager] ServerConfig 누락으로 점수 저장 요청을 스킵합니다.");
            yield break;
        }

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
        if (isConfigMissing){
            Debug.LogWarning("⚠️ [RankingManager] ServerConfig 누락으로 랭킹 조회를 스킵합니다.");
            if (onCallback != null) onCallback(new ScoreData[0]);
            yield break;
        }
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
        if (isConfigMissing){
            Debug.LogWarning("⚠️ [RankingManager] ServerConfig 누락으로 내 랭킹 조회를 스킵합니다.");
            if (onCallback != null) onCallback(null);
            yield break;
        }

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
        if (isConfigMissing){
            IsServerAlive = false;
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(BaseUrl + "/health"))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();
            IsServerAlive = (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text == "ok");
        }
    }

    // 닉네임 단독 동기화 API 호출
    public IEnumerator UpdateNickname(string newName)
    {
        if (isConfigMissing)
        {
            Debug.LogWarning("⚠️ [RankingManager] ServerConfig 누락으로 닉네임 동기화를 스킵합니다.");
            yield break;
        }

        string uuid = GetOrCreateUUID();
        string url = BaseUrl + "/rankings/" + uuid + "/name";

        NicknameUpdateReq req = new NicknameUpdateReq {name = newName };
        string json = JsonUtility.ToJson(req);

        UnityWebRequest request = new UnityWebRequest(url, "PATCH");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log($"<color=green>✅ 닉네임 서버 즉시 동기화 성공: {newName}</color>");
        }
        else
        {
            Debug.LogWarning($"⚠️ 닉네임 동기화 실패 (신규 유저일 수 있음): {request.error}");
        }
    }
}