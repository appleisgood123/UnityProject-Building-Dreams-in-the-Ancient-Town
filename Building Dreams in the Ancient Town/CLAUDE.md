# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A Chinese ancient-town building simulation game built with Unity 2022.3.62. The player places buildings on terrain, manages resources (Silver, Wood, Stone, Happiness, PopulationCap, TechPoints), recruits employees, unlocks technologies, and completes tasks. Win condition: reach 200 Happiness.

## No CLI — Unity Editor Only

There are no CLI commands for build, lint, or test. All development happens inside the Unity Editor. Code is C# (.NET Standard 2.1). Scripts live in `Assets/Scripts/`, data assets in `Assets/Datas/`, runtime-loaded assets in `Assets/Resources/`.

## Architecture: 7 Core Singletons

All managers use `MonoBehaviour` singletons (`Instance` static property, `Awake` self-assignment). Initialization order is not guaranteed — managers reference each other in `OnEnable`/`Start`, so avoid relying on Awake ordering.

| Singleton | Role |
|-----------|------|
| `ResourceManager` | 6 resource types, caps, income multipliers. Fires `OnResourcesChanged` UnityAction. |
| `BuildingManager` | Construct/demolish, monthly income, employee assignment, prerequisite checks. |
| `GameManager` | Employee roster (hire/fire/candidate cycling). DontDestroyOnLoad. |
| `TechManager` | Tech node unlocking, prerequisite resolution, effect application. |
| `TaskManager` | Task progress tracking, completion checks on resource change/tech/building events. |
| `GameMonthManager` | Game time in months, speed 1x–32x (Space to cycle). Fires `OnMonthUpdated` every frame. |
| `GamePauseManager` | Reference-counted pause — multiple UI panels can request pause independently. DontDestroyOnLoad. |

## ScriptableObject-Driven Data

All game data is defined as ScriptableObjects in `Assets/Datas/`:

- **BuildingDataSO** (`Building Data/`) — cost, income, requirement, employee slot definitions. Referenced by BuildingManager, BuildingPageManager, DraggableIcon, TechNodeData.
- **TechNodeData** (`TechData/`) — tech tree nodes with prerequisite SO references (forms a DAG via `List<TechNodeData>`), effect type/value, unlocked building references.
- **TaskDataSO** (`TaskData/`) — task definitions (BuildBuilding / HaveResourceAmount / UnlockTech types).
- **EmployeeTable** (`TableData/`) — master employee definitions.

## Event-Driven Communication

Systems communicate via C# events (`UnityAction`, `System.Action`) rather than direct coupling:
- `ResourceManager.OnResourcesChanged` → ResourcePanel, NPCManager, TaskManager
- `GameMonthManager.OnMonthUpdated` → BuildingManager (accumulates months → applies income)
- `TechManager.OnTechUnlocked` → TaskManager
- `BuildingManager.OnBuildingCountChanged` → TaskManager
- `GamePauseManager.OnPauseChanged` → TechTreeToggle

## Key Patterns

- **Two scenes only**: `Scene0.unity` (start menu, `StartMenu.cs`) and `Scene1.unity` (main game world).
- **Employee assignment**: Buildings declare employee slot types; GameManager provides idle employees by job type; BuildingManager assigns/removes.
- **Building placement**: `DraggableIcon` handles drag-to-place with runtime preview, color-coded valid/invalid overlay, terrain tree removal, and rotation.
- **UI panels**: `MainPanel` is the top-level hub toggling Recruit/Employee/Tech panels via buttons and hotkeys (Y/U/I). Each panel is a self-contained component.
- **Camera**: Q key switches between character-follow and overhead building-placement views via `CameraSwitch`.
- **Time speed**: Space cycles 1x → 2x → 4x → 8x → 16x → 32x → back to 1x.

## When Editing Code

- Comments and in-game text are in Chinese — keep them in Chinese.
- Prefer direct references or singleton access over `FindObjectOfType`.
- ScriptableObjects are the canonical data source — update SO fields, not hardcoded values.
- Managers use `[SerializeField]` for inspector-assigned references; avoid `FindObjectOfType` as a fallback.
- The `BuildingInstance` MonoBehaviour on placed buildings tracks `List<EmployeeData>` assigned to that building.
