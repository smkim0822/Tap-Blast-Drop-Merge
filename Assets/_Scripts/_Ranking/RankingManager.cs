using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
    public bool IsServerAlive { get; private set; } = false;

    private const string UUID_KEY = "PlayerUUID";
    // 로컬 백업용 키값
    private const string LOCAL_HIGH_SCORE_KEY = "BestScore";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        LoadServerConfig();
    }

    void Start()
    {
        StartCoroutine(CheckServerStatus());
    }

    private void LoadServerConfig(){
        config = Resources.Load<ServerConfig>("ServerConfig");

        if (config == null){
            Debug.LogWarning("⚠️ [RankingManager] ServerConfig 파일을 찾을 수 없습니다! 로컬 더미 설정으로 실행됩니다.");
        }
    }

    // --- 공통 네트워크 검증 유틸리티 ---
    private bool CanExecuteNetworkTask()
    {
        if (config == null)
        {
            Debug.LogWarning("⚠️ [RankingManager] ServerConfig가 없어 네트워크 요청을 스킵합니다.");
            return false;
        }
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("⚠️ [RankingManager] 오프라인 상태입니다.");
            return false;
        }
        return true;
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
        }
        return uuid;
    }

    // 오프라인 대비 로컬 점수 백업
    private void SaveScoreLocally(int score)
    {
        int currentLocalHigh = PlayerPrefs.GetInt(LOCAL_HIGH_SCORE_KEY, 0);
        if (score > currentLocalHigh)
        {
            PlayerPrefs.SetInt(LOCAL_HIGH_SCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }

    // 점수 저장
    public IEnumerator PostScore(int score)
    {
        SaveScoreLocally(score);

        if (!CanExecuteNetworkTask()){
            yield break;
        }

        string playerName = NicknameManager.Instance.GetNickname();
        string playerUUID = GetOrCreateUUID();
        string hash = GenerateHash(playerName, score, playerUUID);

        ScoreData data = new ScoreData()
        {
            uuid = playerUUID,
            name = playerName,
            score = score,
            hash = hash
        };
        string json = JsonUtility.ToJson(data);

        using (UnityWebRequest request = CreateJsonRequest(config.GetBaseUrl() + "/rankings", "POST", json))
        {
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
    }

    // Top 50 랭킹 조회
    public IEnumerator GetRanks(Action<bool, ScoreData[]> onCallback)
    {
        if (!CanExecuteNetworkTask()){
            onCallback?.Invoke(false, null);
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(config.GetBaseUrl() + "/rankings"))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                ScoreData[] ranks = JsonHelper.FromJson<ScoreData>(request.downloadHandler.text);
                onCallback?.Invoke(true, ranks);
            }
            else
            {
                Debug.LogError($"❌ 랭킹 조회 실패: {request.error}");
                onCallback?.Invoke(false, null);
            }
        }
    }

    public IEnumerator GetMyRank(Action<bool, MyRankData> onCallback)
    {
        if (!CanExecuteNetworkTask()){
            onCallback?.Invoke(false, null);
            yield break;
        }

        string url = $"{config.GetBaseUrl()}/rankings/{GetOrCreateUUID()}";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                MyRankData myRank = JsonUtility.FromJson<MyRankData>(request.downloadHandler.text);
                onCallback?.Invoke(true, myRank);
            }
            else
            {
                if (request.responseCode == 404)
                {
                    onCallback?.Invoke(true, null);
                }
                else
                {
                    onCallback?.Invoke(false, null);
                }
            }
        }
    }

    // 외부에서 현재 네트워크 상태를 체크하기 위한 메서드
    public bool IsNetworkReached()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    public IEnumerator CheckServerStatus()
    {
        if (!CanExecuteNetworkTask())
        {
            IsServerAlive = false;
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequest.Get(config.GetBaseUrl() + "/health"))
        {
            request.timeout = 3;
            yield return request.SendWebRequest();
            IsServerAlive = (request.result == UnityWebRequest.Result.Success && request.downloadHandler.text == "ok");
        }
    }

    public IEnumerator UpdateNickname(string newName)
    {
        if (!CanExecuteNetworkTask())
        {
            yield break;
        }

        string url = $"{config.GetBaseUrl()}/rankings/{GetOrCreateUUID()}/name";
        NicknameUpdateReq req = new NicknameUpdateReq { name = newName };
        string json = JsonUtility.ToJson(req);

        using (UnityWebRequest request = CreateJsonRequest(url, "PATCH", json))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ 닉네임 서버 즉시 동기화 성공: {newName}");
            }
            else
            {
                Debug.LogWarning($"⚠️ 닉네임 동기화 실패: {request.error}");
            }
        }
    }

    private UnityWebRequest CreateJsonRequest(string url, string method, string jsonBody)
    {
        UnityWebRequest request = new UnityWebRequest(url, method);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 5;
        return request;
    }

    private string GenerateHash(string name, int score, string uuid)
    {
        string secret = config != null ? config.secretKey : "dummy_secret_key";
        string input = $"{name}:{score}:{uuid}:{secret}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}