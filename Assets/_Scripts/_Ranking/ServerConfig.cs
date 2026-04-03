using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfig", menuName = "Configs/ServerConfig")]
public class ServerConfig : ScriptableObject
{
    public enum ServerEnvironment { Local, Production }

    [Header("Server Settings")]
    public ServerEnvironment environment = ServerEnvironment.Production;
    public string oracleIp;

    [Header("Security")]
    public string secretKey;

    // 환경에 따른 URL 자동 반환 로직 추가
    public string GetBaseUrl()
    {
        if (environment == ServerEnvironment.Local)
        {
            return "http://localhost:8080/api";
        }
        
        return $"https://{oracleIp}/api";
    }
}