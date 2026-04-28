using System.Text;
using UnityEngine.Networking;

/// <summary>
/// 랭킹 API용 UnityWebRequest 생성 책임을 분리합니다.
/// </summary>
public static class RankingWebRequestFactory
{
    public static UnityWebRequest CreateJsonRequest(string url, string method, string jsonBody, int timeoutSeconds = 5)
    {
        UnityWebRequest request = new UnityWebRequest(url, method);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = timeoutSeconds;

        return request;
    }
}
