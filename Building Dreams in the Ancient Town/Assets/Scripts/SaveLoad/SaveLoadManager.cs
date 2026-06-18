using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveLoadManager
{
    public const int MAX_SLOTS = 3;
    public static SaveData PendingLoadData;

    private static string SavePath(int slot) =>
        Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");

    public static bool HasSave(int slot) => File.Exists(SavePath(slot));

    public static SaveData LoadSave(int slot)
    {
        string path = SavePath(slot);
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void SaveGame(int slot, SaveData data)
    {
        data.slotIndex = slot;
        data.saveTime = System.DateTime.Now.ToString("yyyy/MM/dd HH:mm");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath(slot), json);
    }

    public static void DeleteSave(int slot)
    {
        string path = SavePath(slot);
        if (File.Exists(path)) File.Delete(path);
    }

    public static SaveData CaptureCurrentState()
    {
        SaveData data = new SaveData();

        // Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            data.playerPosX = pos.x;
            data.playerPosY = pos.y;
            data.playerPosZ = pos.z;
            data.playerRotY = player.transform.eulerAngles.y;
        }
        else
        {
            Debug.LogWarning("[存档] 未找到 Player (Tag=Player)");
        }

        // Resources
        if (ResourceManager.Instance != null)
        {
            data.silver = ResourceManager.Instance.Silver;
            data.wood = ResourceManager.Instance.Wood;
            data.stone = ResourceManager.Instance.Stone;
            data.happiness = ResourceManager.Instance.Happiness;
            data.populationCap = ResourceManager.Instance.PopulationCap;
            data.techPoints = ResourceManager.Instance.TechPoints;
            data.woodCap = ResourceManager.Instance.WoodCap;
            data.stoneCap = ResourceManager.Instance.StoneCap;
            data.silverIncomeMultiplier = ResourceManager.Instance.silverIncomeMultiplier;
            data.woodIncomeMultiplier = ResourceManager.Instance.woodIncomeMultiplier;
            data.stoneIncomeMultiplier = ResourceManager.Instance.stoneIncomeMultiplier;
        }

        // Time
        if (GameMonthManager.Instance != null)
            data.totalMonths = GameMonthManager.Instance.TotalMonths;

        // Tech
        if (TechManager.Instance != null)
        {
            foreach (var tech in TechManager.Instance.GetAllTechNodes())
            {
                if (TechManager.Instance.IsUnlocked(tech))
                    data.unlockedTechNames.Add(tech.nodeName);
            }
        }

        // Buildings - capture all BuildingInstance in scene (both dynamic and scene-placed)
        BuildingInstance[] allInstances = Object.FindObjectsOfType<BuildingInstance>();
        foreach (var inst in allInstances)
        {
            if (inst == null || inst.data == null) continue;
            var entry = new BuildingSaveEntry
            {
                buildingName = inst.data.buildingName,
                posX = inst.transform.position.x,
                posY = inst.transform.position.y,
                posZ = inst.transform.position.z,
                rotX = inst.transform.rotation.x,
                rotY = inst.transform.rotation.y,
                rotZ = inst.transform.rotation.z,
                rotW = inst.transform.rotation.w,
                assignedEmployeeUIDs = new List<string>(inst.assignedEmployeeUIDs)
            };
            data.buildings.Add(entry);
        }
        Debug.Log($"[存档] 捕获建筑: {data.buildings.Count} 个");

        // Employees
        if (GameManager.Instance != null)
        {
            foreach (var emp in GameManager.Instance.GetEmployeeList())
            {
                data.employees.Add(new EmployeeSaveEntry
                {
                    uid = emp.uid,
                    id = emp.id,
                    employeeName = emp.employeeName,
                    jobType = (int)emp.jobType,
                    assignedBuildingUID = emp.assignedBuildingUID
                });
            }
        }

        // Tasks
        if (TaskManager.Instance != null)
        {
            foreach (var t in TaskManager.Instance.activeTasks)
                data.activeTaskNames.Add(t.taskName);
            foreach (var t in TaskManager.Instance.completedTasks)
                data.completedTaskNames.Add(t.taskName);
        }

        return data;
    }

    public static void ApplySaveData(SaveData data)
    {
        // Resources
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.LoadFromSaveData(data);

        // Time
        if (GameMonthManager.Instance != null)
            GameMonthManager.Instance.SetTotalMonths(data.totalMonths);

        // Tech
        if (TechManager.Instance != null)
            TechManager.Instance.LoadUnlockedTechs(data.unlockedTechNames);

        // Buildings - destroy all existing BuildingInstance objects, then restore
        if (BuildingManager.Instance != null)
        {
            // Destroy ALL scene building instances (both pre-placed and runtime)
            BuildingInstance[] existingInstances = Object.FindObjectsOfType<BuildingInstance>();
            foreach (var inst in existingInstances)
            {
                if (inst != null)
                {
                    if (inst.currentNPC != null)
                        Object.Destroy(inst.currentNPC);
                    Object.Destroy(inst.gameObject);
                }
            }
            BuildingManager.Instance.ClearAllBuildings();
            foreach (var entry in data.buildings)
            {
                BuildingDataSO buildingData = FindBuildingDataByName(entry.buildingName);
                if (buildingData != null && buildingData.finalPrefab != null)
                {
                    Vector3 pos = new Vector3(entry.posX, entry.posY, entry.posZ);
                    Quaternion rot = new Quaternion(entry.rotX, entry.rotY, entry.rotZ, entry.rotW);
                    BuildingManager.Instance.RestoreBuilding(buildingData, pos, rot, entry.assignedEmployeeUIDs);
                }
            }
        }

        // Employees
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearAllEmployees();
            foreach (var entry in data.employees)
            {
                GameManager.Instance.RestoreEmployee(entry);
            }
        }

        // Tasks
        if (TaskManager.Instance != null)
            TaskManager.Instance.LoadFromSaveData(data.activeTaskNames, data.completedTaskNames);

        // Player position (do last - after buildings are placed)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 savedPos = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.position = savedPos;
                rb.velocity = Vector3.zero;
            }
            else
            {
                player.transform.position = savedPos;
            }
            player.transform.eulerAngles = new Vector3(0, data.playerRotY, 0);
        }
    }

    private static BuildingDataSO FindBuildingDataByName(string name)
    {
        // Search from BuildingPageManager's allBuildings list (inspector-assigned)
        BuildingPageManager pageManager = Object.FindObjectOfType<BuildingPageManager>();
        if (pageManager != null && pageManager.allBuildings != null)
        {
            foreach (var b in pageManager.allBuildings)
            {
                if (b != null && b.buildingName == name) return b;
            }
        }
        return null;
    }
}
