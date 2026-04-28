using System;
using UnityEngine;

/// <summary>
/// 랭킹 서버에서 플레이어를 식별할 UUID를 관리합니다.
/// </summary>
public static class RankingPlayerIdentityStore
{
    const string UuidKey = "PlayerUUID";

    public static string GetOrCreateUuid()
    {
        string uuid = PlayerPrefs.GetString(UuidKey, "");

        if (!string.IsNullOrEmpty(uuid))
        {
            return uuid;
        }

        uuid = Guid.NewGuid().ToString();
        PlayerPrefs.SetString(UuidKey, uuid);
        PlayerPrefs.Save();
        return uuid;
    }
}
