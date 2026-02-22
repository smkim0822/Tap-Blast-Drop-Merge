using UnityEngine;

[CreateAssetMenu(fileName = "ServerConfig", menuName = "Configs/ServerConfig")]
public class ServerConfig : ScriptableObject
{
    public string oracleIp;
    public string secretKey;
}