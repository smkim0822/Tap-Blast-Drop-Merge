using UnityEngine;

/// <summary>
/// 서버 연결 정보를 저장하고 관리하는 ScriptableObject.
/// 프로젝트 내에서 우클릭 -> Create -> Configs -> ServerConfig를 통해 생성.
/// </summary>
[CreateAssetMenu(fileName = "ServerConfig", menuName = "Configs/ServerConfig")]
public class ServerConfig : ScriptableObject
{
    /// <summary>
    /// 서버 접속 환경 구분용 열거형.
    /// </summary>
    public enum ServerEnvironment 
    { 
        Local,          // 로컬 테스트 환경 (localhost)
        Production      // 실제 운영 환경
    }

    [Header("Server Settings (서버 설정)")]
    [Tooltip("접속할 서버 환경을 선택. (Local: 테스트용, Production: 실서비스용)")]
    public ServerEnvironment environment = ServerEnvironment.Production;

    [Tooltip("실제 운영 서버 주소. 도메인만(예: pd-ranking.duckdns.org) 입력하거나, 권장 형식인 풀 URL(예: https://pd-ranking.duckdns.org)을 입력합니다.")]
    public string oracleIp;

    [Tooltip("운영 서버 포트 (포트 미입력 시 기본값). 풀 URL(http(s)://...)을 입력했다면 무시됩니다.")]
    public int productionPort = 8080;

    [Header("Security (보안)")]
    [Tooltip("네트워크 데이터 위변조를 막기 위한 암호화용 시크릿 키.")]
    public string secretKey;

    /// <summary>
    /// 현재 설정된 환경에 맞는 서버의 기본 API 주소를 반환.
    /// </summary>
    /// <returns>서버 API Base URL 문자열</returns>
    public string GetBaseUrl()
    {
        // 로컬 환경 테스트용 주소
        if (environment == ServerEnvironment.Local)
        {
            return "http://localhost:8080/api";
        }

        // 실제 운영 환경용 주소
        // - oracleIp에 도메인(예: "pd-ranking.duckdns.org")만 넣어도 동작하도록 기본값을 보정합니다.
        // - 이미 "http(s)://..." 형태(권장)로 입력되면 그대로 사용합니다.
        // - 스킴이 없으면 http로 시작하고, 포트도 없으면 Spring 기본(productionPort)을 붙입니다.
        // - /api가 없으면 붙입니다.
        if (string.IsNullOrWhiteSpace(oracleIp))
        {
            // 실수로 비워둔 경우를 대비한 안전한 기본값(로컬은 아님). HTTPS 도메인을 사용.
            return "https://pd-ranking.duckdns.org/api";
        }

        string raw = oracleIp.Trim();
        bool hasScheme = raw.StartsWith("http://") || raw.StartsWith("https://");
        bool hasExplicitPort = raw.Contains(":");

        // scheme이 없으면 http로 시작하고, 포트도 없으면 productionPort를 붙입니다.
        // (duckdns 같은 도메인을 포트 없이 입력하면 80으로 붙어서 서버(8080)와 불일치가 자주 발생)
        string baseUrl;
        if (!hasScheme)
        {
            baseUrl = hasExplicitPort ? $"http://{raw}" : $"http://{raw}:{productionPort}";
        }
        else
        {
            baseUrl = raw;
        }

        // /api 보정 (이미 있으면 중복 방지)
        if (!baseUrl.Contains("/api"))
        {
            baseUrl += "/api";
        }

        return baseUrl;
    }
}