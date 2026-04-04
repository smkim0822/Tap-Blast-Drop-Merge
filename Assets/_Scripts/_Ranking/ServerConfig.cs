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

    [Tooltip("실제 운영 서버의 도메인 입력.")]
    public string oracleIp;

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
        return $"https://{oracleIp}/api";
    }
}