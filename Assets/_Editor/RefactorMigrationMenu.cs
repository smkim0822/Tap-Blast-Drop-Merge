#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 리팩토링 이후 "숨김 레거시 필드 -> 새 컴포넌트"로 씬 참조를 이관하는 도구입니다.
/// 한 번 실행 후 씬을 저장하면 런타임 자동 이관 의존을 줄일 수 있습니다.
/// </summary>
public static class RefactorMigrationMenu
{
    [MenuItem("Dev/Migrate Refactor References (Active Scene)")]
    public static void MigrateActiveScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            Debug.LogError("[Migrate] 활성 씬을 찾을 수 없습니다.");
            return;
        }

        int migratedCount = 0;
        foreach (var root in scene.GetRootGameObjects())
        {
            var gameManagers = root.GetComponentsInChildren<GameManager>(true);
            foreach (var gm in gameManagers)
            {
                if (MigrateGameManager(gm)) migratedCount++;
            }

            var scoreManagers = root.GetComponentsInChildren<ScoreManager>(true);
            foreach (var sm in scoreManagers)
            {
                if (MigrateScoreManager(sm)) migratedCount++;
            }

            var spawners = root.GetComponentsInChildren<Spawner>(true);
            foreach (var sp in spawners)
            {
                if (MigrateSpawner(sp)) migratedCount++;
            }

            var rankingPopups = root.GetComponentsInChildren<RankingPopup>(true);
            foreach (var rp in rankingPopups)
            {
                if (MigrateRankingPopup(rp)) migratedCount++;
            }
        }

        if (migratedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }

        Debug.Log($"[Migrate] 완료. 마이그레이션 대상 오브젝트 수: {migratedCount}. 씬을 저장하세요.");
    }

    static bool MigrateGameManager(GameManager gm)
    {
        if (gm == null) return false;

        var gmSO = new SerializedObject(gm);
        var pMainPanel = gmSO.FindProperty("mainPanel");
        var pGameOverPanel = gmSO.FindProperty("gameOverPanel");
        var pGamePlayGroup = gmSO.FindProperty("gamePlayGroup");
        var pGamePlayUI = gmSO.FindProperty("gamePlayUI");
        var pPlayer = gmSO.FindProperty("player");
        var pPlayerStartPos = gmSO.FindProperty("playerStartPos");

        // 청산 이후 레거시 필드가 삭제된 경우
        if (pMainPanel == null && pGameOverPanel == null && pGamePlayGroup == null && pGamePlayUI == null && pPlayer == null && pPlayerStartPos == null)
        {
            return false;
        }

        // 레거시 값이 전부 비어있으면 작업할 게 없음
        if (IsAllNull(pMainPanel, pGameOverPanel, pGamePlayGroup, pGamePlayUI, pPlayer, pPlayerStartPos))
        {
            return false;
        }

        var roots = gm.GetComponent<GameUIRoots>();
        if (roots == null)
        {
            roots = gm.gameObject.AddComponent<GameUIRoots>();
        }

        var rootsSO = new SerializedObject(roots);
        rootsSO.FindProperty("_mainPanel").objectReferenceValue = pMainPanel.objectReferenceValue;
        rootsSO.FindProperty("_gameOverPanel").objectReferenceValue = pGameOverPanel.objectReferenceValue;
        rootsSO.FindProperty("_gamePlayGroup").objectReferenceValue = pGamePlayGroup.objectReferenceValue;
        rootsSO.FindProperty("_gamePlayUI").objectReferenceValue = pGamePlayUI.objectReferenceValue;
        rootsSO.FindProperty("_player").objectReferenceValue = pPlayer.objectReferenceValue;
        rootsSO.FindProperty("_playerStartPos").objectReferenceValue = pPlayerStartPos.objectReferenceValue;
        rootsSO.ApplyModifiedPropertiesWithoutUndo();

        // 레거시 필드 제거(비움) — 씬 저장 시 더 이상 GameManager에 남지 않음
        pMainPanel.objectReferenceValue = null;
        pGameOverPanel.objectReferenceValue = null;
        pGamePlayGroup.objectReferenceValue = null;
        pGamePlayUI.objectReferenceValue = null;
        pPlayer.objectReferenceValue = null;
        pPlayerStartPos.objectReferenceValue = null;
        gmSO.ApplyModifiedPropertiesWithoutUndo();

        return true;
    }

    static bool MigrateScoreManager(ScoreManager sm)
    {
        if (sm == null) return false;

        var smSO = new SerializedObject(sm);
        var pCurrentText = smSO.FindProperty("currentScoreText");
        var pBestText = smSO.FindProperty("bestScoreText");
        var pPunchScale = smSO.FindProperty("punchScale");
        var pDuration = smSO.FindProperty("duration");
        var pBestPunchScale = smSO.FindProperty("bestPunchScale");

        // 청산 이후 레거시 필드가 삭제된 경우
        if (pCurrentText == null && pBestText == null && pPunchScale == null && pDuration == null && pBestPunchScale == null) return false;
        if (IsAllNull(pCurrentText, pBestText)) return false;

        var hud = sm.GetComponent<ScoreHudView>();
        if (hud == null)
        {
            hud = sm.gameObject.AddComponent<ScoreHudView>();
        }

        var hudSO = new SerializedObject(hud);
        hudSO.FindProperty("currentScoreText").objectReferenceValue = pCurrentText.objectReferenceValue;
        hudSO.FindProperty("bestScoreText").objectReferenceValue = pBestText.objectReferenceValue;
        hudSO.FindProperty("punchScale").floatValue = pPunchScale.floatValue;
        hudSO.FindProperty("duration").floatValue = pDuration.floatValue;
        hudSO.FindProperty("bestPunchScale").floatValue = pBestPunchScale.floatValue;
        hudSO.ApplyModifiedPropertiesWithoutUndo();

        pCurrentText.objectReferenceValue = null;
        pBestText.objectReferenceValue = null;
        smSO.ApplyModifiedPropertiesWithoutUndo();

        return true;
    }

    static bool MigrateSpawner(Spawner sp)
    {
        if (sp == null) return false;

        var spSO = new SerializedObject(sp);
        var pNextUis = spSO.FindProperty("nextBombUIs");
        var pGuideLine = spSO.FindProperty("guideLine");
        var pGroundY = spSO.FindProperty("groundY");
        var pLineScale = spSO.FindProperty("lineTextureScale");
        var pSpawnCurve = spSO.FindProperty("spawnCurve");
        var pSpawnDuration = spSO.FindProperty("spawnDuration");
        var pLoadCurve = spSO.FindProperty("spawnerLoadCurve");
        var pLoadDuration = spSO.FindProperty("spawnerLoadDuration");
        var pUiCurve = spSO.FindProperty("uiPunchCurve");
        var pUiDuration = spSO.FindProperty("uiAnimDuration");

        // 청산 이후 레거시 필드가 삭제된 경우
        if (pNextUis == null && pGuideLine == null && pGroundY == null && pLineScale == null && pSpawnCurve == null && pSpawnDuration == null &&
            pLoadCurve == null && pLoadDuration == null && pUiCurve == null && pUiDuration == null)
        {
            return false;
        }

        if (IsAllNull(pNextUis, pGuideLine) && pGroundY == null) return false;

        var visuals = sp.GetComponent<SpawnerVisuals>();
        if (visuals == null)
        {
            visuals = sp.gameObject.AddComponent<SpawnerVisuals>();
        }

        var vSO = new SerializedObject(visuals);
        CopyObjectReferenceArray(pNextUis, vSO.FindProperty("nextBombUIs"));
        vSO.FindProperty("guideLine").objectReferenceValue = pGuideLine.objectReferenceValue;
        vSO.FindProperty("groundY").floatValue = pGroundY.floatValue;
        vSO.FindProperty("lineTextureScale").floatValue = pLineScale.floatValue;
        vSO.FindProperty("spawnCurve").animationCurveValue = pSpawnCurve.animationCurveValue;
        vSO.FindProperty("spawnDuration").floatValue = pSpawnDuration.floatValue;
        vSO.FindProperty("spawnerLoadCurve").animationCurveValue = pLoadCurve.animationCurveValue;
        vSO.FindProperty("spawnerLoadDuration").floatValue = pLoadDuration.floatValue;
        vSO.FindProperty("uiPunchCurve").animationCurveValue = pUiCurve.animationCurveValue;
        vSO.FindProperty("uiAnimDuration").floatValue = pUiDuration.floatValue;
        vSO.ApplyModifiedPropertiesWithoutUndo();

        // 레거시 값 비움
        if (pNextUis != null && pNextUis.isArray) pNextUis.arraySize = 0;
        pGuideLine.objectReferenceValue = null;
        spSO.ApplyModifiedPropertiesWithoutUndo();

        return true;
    }

    static bool MigrateRankingPopup(RankingPopup rp)
    {
        if (rp == null) return false;

        var rpSO = new SerializedObject(rp);
        var pContentParent = rpSO.FindProperty("contentParent");
        var pItemPrefab = rpSO.FindProperty("itemPrefab");
        var pOffline = rpSO.FindProperty("offlineMessageObject");
        var pMyName = rpSO.FindProperty("myNameText");
        var pMyRank = rpSO.FindProperty("myRankText");
        var pMyScore = rpSO.FindProperty("myScoreText");

        // 청산 이후 레거시 필드가 삭제된 경우
        if (pContentParent == null && pItemPrefab == null && pOffline == null && pMyName == null && pMyRank == null && pMyScore == null)
        {
            return false;
        }

        if (IsAllNull(pContentParent, pItemPrefab, pOffline, pMyName, pMyRank, pMyScore))
        {
            return false;
        }

        var view = rp.GetComponent<RankingBoardView>();
        if (view == null)
        {
            view = rp.gameObject.AddComponent<RankingBoardView>();
        }

        var vSO = new SerializedObject(view);
        vSO.FindProperty("contentParent").objectReferenceValue = pContentParent.objectReferenceValue;
        vSO.FindProperty("itemPrefab").objectReferenceValue = pItemPrefab.objectReferenceValue;
        vSO.FindProperty("offlineMessageObject").objectReferenceValue = pOffline.objectReferenceValue;
        vSO.FindProperty("myNameText").objectReferenceValue = pMyName.objectReferenceValue;
        vSO.FindProperty("myRankText").objectReferenceValue = pMyRank.objectReferenceValue;
        vSO.FindProperty("myScoreText").objectReferenceValue = pMyScore.objectReferenceValue;
        vSO.ApplyModifiedPropertiesWithoutUndo();

        pContentParent.objectReferenceValue = null;
        pItemPrefab.objectReferenceValue = null;
        pOffline.objectReferenceValue = null;
        pMyName.objectReferenceValue = null;
        pMyRank.objectReferenceValue = null;
        pMyScore.objectReferenceValue = null;
        rpSO.ApplyModifiedPropertiesWithoutUndo();

        return true;
    }

    static bool IsAllNull(params SerializedProperty[] props)
    {
        foreach (var p in props)
        {
            if (p == null) continue;
            if (HasAnyObjectReference(p)) return false;
        }
        return true;
    }

    static bool HasAnyObjectReference(SerializedProperty prop)
    {
        if (prop == null) return false;

        if (prop.propertyType == SerializedPropertyType.ObjectReference)
        {
            return prop.objectReferenceValue != null;
        }

        if (prop.isArray)
        {
            for (int i = 0; i < prop.arraySize; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                if (element != null && element.propertyType == SerializedPropertyType.ObjectReference && element.objectReferenceValue != null)
                {
                    return true;
                }
            }
        }

        return false;
    }

    static void CopyObjectReferenceArray(SerializedProperty sourceArray, SerializedProperty destArray)
    {
        if (sourceArray == null || destArray == null) return;
        if (!sourceArray.isArray || !destArray.isArray) return;

        destArray.arraySize = sourceArray.arraySize;
        for (int i = 0; i < sourceArray.arraySize; i++)
        {
            var s = sourceArray.GetArrayElementAtIndex(i);
            var d = destArray.GetArrayElementAtIndex(i);
            if (s != null && d != null && d.propertyType == SerializedPropertyType.ObjectReference)
            {
                d.objectReferenceValue = s.objectReferenceValue;
            }
        }
    }
}
#endif

