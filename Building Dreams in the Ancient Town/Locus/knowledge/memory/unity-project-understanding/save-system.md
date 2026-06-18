---
id: kd_7745e16a-1537-44b0-b884-28074284991a
type: memory
path: unity-project-understanding/save-system.md
title: save-system
inheritInjectMode: true
summaryEnabled: true
commandEnabled: false
readOnly: false
inheritAiConfig: true
createdAt: 1781761440378
updatedAt: 1781761440379
---

# save-system

## Summary
Save system architecture, flows, known bugs and fixes

<!-- locus:body:start -->
## Save System Architecture

### Core Files
- `Assets/Scripts/SaveLoad/SaveData.cs` — Data model (`SaveData`, `BuildingSaveEntry`, `EmployeeSaveEntry`)
- `Assets/Scripts/SaveLoad/SaveLoadManager.cs` — Static save/load/capture/apply logic
- `Assets/Scripts/UI/GameSettings.cs` — In-game ESC menu with save/load UI (SaveOverlay)
- `Assets/Scripts/SaveLoad/LoadPanelController.cs` — Main menu load panel
- `Assets/Scripts/StartUI/StartMenu.cs` — Main menu (StartGame sets PendingLoadData=null)

### Serialization
- JSON via `JsonUtility`, stored at `Application.persistentDataPath`/`save_slot_{0,1,2}.json` (3 slots)

### Save Flow
1. `SaveLoadManager.CaptureCurrentState()` gathers state from all managers via FindObjectsOfType / FindGameObjectWithTag
2. `SaveLoadManager.SaveGame(slot, data)` writes JSON

### Load Flow (Main Menu → Scene1)
1. `LoadPanelController` sets `PendingLoadData`, loads Scene1
2. Scene1 has no pre-existing GameManager, so fresh init works
3. `ResourceManager.Start()` coroutine detects PendingLoadData, waits 1 frame, calls `ApplySaveData`

### Load Flow (In-Game)
1. `GameSettings.OnLoadSaveInGame()` sets `PendingLoadData`, destroys GameManager, loads Scene1
2. Fixed: uses `DestroyImmediate` to prevent DontDestroyOnLoad race condition

### ApplySaveData Order
1. Resources (ResourceManager.LoadFromSaveData)
2. Time (GameMonthManager.SetTotalMonths)
3. Tech (TechManager.LoadUnlockedTechs)
4. Buildings: destroy all BuildingInstance → ClearAllBuildings → restore each
5. Employees: clear all → restore each
6. Tasks: load active/completed
7. Player position (last, after buildings placed)

### Singleton Pattern (On GameManager GameObject)
All managers are on the same GameObject: `GameManager`, `ResourceManager`, `BuildingManager`, `GameMonthManager`, `MouseManager`, `CameraSwitch`, `TechManager`, `GamePauseManager`, `TaskManager`, `NPCManager`, `DialogueManager`, `GameCompletion`
- `GameManager` uses private `_instance` with standard null check
- `ResourceManager` uses public `Instance` with standard null check
- Both call `DontDestroyOnLoad(gameObject)`

### Known Issues Fixed
1. In-game load: `Destroy` (deferred) + immediate `LoadScene` caused old DontDestroyOnLoad GameManager to still be alive when new scene's Awake ran → new GameManager destroyed itself
2. Building cleanup: original code only destroyed children of "Buildings" GameObject + ClearAllBuildings list. Pre-placed buildings not under "Buildings" and not in allBuildingInstances were missed → duplicates on load. Fixed: FindObjectsOfType<BuildingInstance> to destroy ALL.

### Scene Building Status
- Scene1 has ~48 pre-placed buildings with BuildingInstance component (under `Buildings/` and `Environment/MainGround/`)
- Pre-placed buildings have BuildingInstance added as scene override (not on prefab)
- BuildingInstance.data references BuildingDataSO assets
- `buildingName` field (Chinese names like "南阳府署") is used for lookup during restore via `BuildingPageManager.allBuildings`
<!-- locus:body:end -->
