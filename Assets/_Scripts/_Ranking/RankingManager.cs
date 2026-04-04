using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 서버에 전송하거나 받아올 랭킹 데이터 구조체.
/// </summary>
[System.Serializable]
public class ScoreData
{
    public string uuid;
    public string name;
    public int score;
    public string hash;
}

/// <summary>
/// 서버로부터 내 랭킹 정보를 받아올 때 사용하는 데이터 구조체.
/// </summary>
[System.Serializable]
public class MyRankData
{
    public int rank;
    public string name;
    public int score;
}

/// <summary>
/// 서버에 닉네임 변경 요청을 보낼 때 사용하는 데이터 구조체.
/// </summary>
[System.Serializable]
public class NicknameUpdateReq
{
    public string name;
}

/// <summary>
/// 랭킹 서버와의 통신, 데이터 해싱, 로컬 점수 백업 등을 관리하는 매니저 클래스.
/// </summary>
public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;

    private ServerConfig config;

    /// <summary>
    /// 현재 서버가 정상적으로 응답하고 있는지 여부.
    /// </summary>
    public bool IsServerAlive { get; private set; } = false;

    // 로컬 데이터 저장용 키값
    private const string UUID_KEY = "PlayerUUID";
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
        // 시작 시 서버 상태 체크
        StartCoroutine(CheckServerStatus());
    }

    /// <summary>
    /// Resources 폴더에서 서버 주소 및 시크릿 키가 담긴 ScriptableObject를 불러옴.
    /// </summary>
    private void LoadServerConfig(){
        config = Resources.Load<ServerConfig>("ServerConfig");

        if (config == null){
            Debug.LogWarning("⚠️ [RankingManager] ServerConfig 파일을 찾을 수 없습니다! 로컬 더미 설정으로 실행됩니다.");
        }
    }

    /// <summary>
    /// 네트워크 요청을 보내기 전, 통신이 가능한 상태인지 검증.
    /// </summary>
    /// <returns>통신 가능 여부</returns>
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

    /// <summary>
    /// 현재 기기의 UUID를 가져오거나, 없다면 새로 생성하여 저장.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 오프라인 상태를 대비하여 현재 점수를 로컬에 최고 점수로 백업.
    /// </summary>
    /// <param name="score">저장할 점수</param>
    private void SaveScoreLocally(int score)
    {
        int currentLocalHigh = PlayerPrefs.GetInt(LOCAL_HIGH_SCORE_KEY, 0);
        if (score > currentLocalHigh)
        {
            PlayerPrefs.SetInt(LOCAL_HIGH_SCORE_KEY, score);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 서버로 플레이어의 점수를 전송. (오프라인일 경우 로컬에만 저장)
    /// </summary>
    /// <param name="score">달성한 점수</param>
    /// <returns></returns>
    public IEnumerator PostScore(int score)
    {
        // 로컬에 먼저 백업
        SaveScoreLocally(score);

        // 네트워크 불가 시 서버 통신 중단
        if (!CanExecuteNetworkTask()){
            yield break;
        }

        string playerName = NicknameManager.Instance.GetNickname();
        string playerUUID = GetOrCreateUUID();
        // 보안: 데이터 위변조 방지용 해시 생성
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

    /// <summary>
    /// 서버에서 전체 상위 랭킹 정보를 가져옴.
    /// </summary>
    /// <param name="onCallback">완료 시 호출될 콜백 (성공 여부, 랭킹 배열 데이터)</param>
    /// <returns></returns>
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
                // 최상위 JSON 배열을 파싱하기 위해 JsonHelper 사용
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

    /// <summary>
    /// 서버에서 현재 플레이어의 랭킹 정보를 가져옴.
    /// </summary>
    /// <param name="onCallback">완료 시 호출될 콜백 (성공 여부, 내 랭킹 데이터)</param>
    /// <returns></returns>
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

    /// <summary>
    /// 외부 스크립트에서 현재 기기가 인터넷에 연결되어 있는지 단순 체크할 때 사용.
    /// </summary>
    /// <returns>네트워크 연결 여부</returns>
    public bool IsNetworkReached()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    /// <summary>
    /// 서버가 정상 가동 중인지 Health Check API를 통해 확인.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 닉네임 변경 시, 서버의 기존 랭킹 데이터에도 새 닉네임을 동기화.
    /// </summary>
    /// <param name="newName">변경할 새 닉네임</param>
    /// <returns></returns>
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

    /// <summary>
    /// JSON 데이터를 Body에 담아 보내는 UnityWebRequest 객체를 생성하는 유틸리티 메서드.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="method"></param>
    /// <param name="jsonBody"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 데이터 변조를 막기 위해, 데이터와 시크릿 키를 조합하여 SHA256 해시를 생성.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="score"></param>
    /// <param name="uuid"></param>
    /// <returns></returns>
    private string GenerateHash(string name, int score, string uuid)
    {
        // 보안을 위해 ServerConfig의 시크릿 키 사용
        string secret = config != null ? config.secretKey : "dummy_secret_key";
        string input = $"{name}:{score}:{uuid}:{secret}";

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();

            // 바이트 배열을 16진수 문자열로 변환
            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}