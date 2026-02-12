using System.Collections;
using System.Security.Cryptography; // 암호화 도구
using System.Text;
using UnityEngine;
using UnityEngine.Networking; // 인터넷 연결 도구

// 1. 서버와 주고받을 데이터 형태 (DTO)
[System.Serializable]
public class ScoreData
{
    public string name;  // 유저 이름
    public int score;    // 점수
    public string hash;  // 보안 검증용 암호
}

public class RankingManager : MonoBehaviour
{
    // 로컬 서버 주소 (나중에는 실제 서버 주소로 바뀜)
    private const string BASE_URL = "http://localhost:8080/api";
    
    // 🔥 중요: 서버(Java)에 적은 비밀키와 100% 똑같아야 함!
    private const string SECRET_KEY = "UnityMergeGameSecretKey2026!"; 

    void Start()
    {
        // 테스트: 게임 시작하자마자 "TestUser"가 100점을 먹었다고 가정하고 전송
        Debug.Log("서버 통신 테스트 시작...");
        StartCoroutine(PostScore("TestUser", 100));
    }

    // --- [기능 1] 점수 저장 (보안 적용) ---
    public IEnumerator PostScore(string playerName, int score)
    {
        // 1. 해시(암호) 생성
        string hash = GenerateHash(playerName, score);

        // 2. 보낼 데이터 포장
        ScoreData data = new ScoreData();
        data.name = playerName;
        data.score = score;
        data.hash = hash;

        // 3. JSON 변환
        string json = JsonUtility.ToJson(data);

        // 4. 서버로 발사! (POST)
        UnityWebRequest request = new UnityWebRequest(BASE_URL + "/scores", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest(); // 응답 올 때까지 대기

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("✅ 점수 저장 성공!");
            // 저장이 성공했으니 랭킹을 다시 불러옴
            StartCoroutine(GetRanks());
        }
        else
        {
            Debug.LogError("❌ 점수 저장 실패: " + request.downloadHandler.text);
        }
    }

    // --- [기능 2] 랭킹 조회 ---
    public IEnumerator GetRanks()
    {
        // 서버에 "랭킹 줘!" 요청 (GET)
        UnityWebRequest request = UnityWebRequest.Get(BASE_URL + "/ranks");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResult = request.downloadHandler.text;
            Debug.Log("📩 랭킹 데이터 수신: " + jsonResult);

            // 아까 만든 JsonHelper로 배열을 분해
            ScoreData[] ranks = JsonHelper.FromJson<ScoreData>(jsonResult);

            // 잘 들어왔는지 로그 찍어보기
            for (int i = 0; i < ranks.Length; i++)
            {
                Debug.Log($"{i + 1}등: {ranks[i].name} ({ranks[i].score}점)");
            }
        }
        else
        {
            Debug.LogError("❌ 랭킹 조회 실패: " + request.error);
        }
    }

    // --- [보안] 해시 생성 함수 (SHA-256) ---
    private string GenerateHash(string name, int score)
    {
        // 이름 + 점수 + 비밀키를 합쳐서 지문을 만듦
        string input = name + score + SECRET_KEY;
        SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hashBytes = sha256.ComputeHash(bytes);

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            builder.Append(hashBytes[i].ToString("x2"));
        }
        return builder.ToString();
    }
}