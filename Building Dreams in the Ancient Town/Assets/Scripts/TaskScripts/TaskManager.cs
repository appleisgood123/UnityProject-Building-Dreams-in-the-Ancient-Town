using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TaskManager : MonoBehaviour
{
    public static TaskManager Instance;

    [Header("任务列表")]
    public List<TaskDataSO> activeTasks = new List<TaskDataSO>();
    public List<TaskDataSO> completedTasks = new List<TaskDataSO>();

    public System.Action<TaskDataSO> OnTaskProgress;   // 任务进度更新时触发
    public System.Action<TaskDataSO> OnTaskCompleted;  // 任务完成时触发

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        // 订阅资源变化事件
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged += CheckTaskProgress;
        // 订阅科技解锁事件
        if (TechManager.Instance != null)
            TechManager.Instance.OnTechUnlocked += OnTechUnlocked;
        // 如果没有建筑建造完成事件，我们会在 BuildingManager.ConstructBuilding 中直接调用 CheckTaskProgress
    }

    private void OnDisable()
    {
        if (ResourceManager.Instance != null)
            ResourceManager.Instance.OnResourcesChanged -= CheckTaskProgress;
        if (TechManager.Instance != null)
            TechManager.Instance.OnTechUnlocked -= OnTechUnlocked;
    }

    // 添加新任务
    public void AddTask(TaskDataSO task)
    {
        if (!activeTasks.Contains(task) && !completedTasks.Contains(task))
        {
            activeTasks.Add(task);
            Debug.Log($"任务添加：{task.taskName}");
            OnTaskProgress?.Invoke(task);
        }
    }

    // 检查所有活跃任务（可由外部事件触发）
    public void LoadFromSaveData(List<string> activeNames, List<string> completedNames)
    {
        activeTasks.Clear();
        completedTasks.Clear();
        TaskDataSO[] allTasks = Resources.LoadAll<TaskDataSO>("");
        foreach (var name in activeNames)
        {
            foreach (var t in allTasks)
            {
                if (t.taskName == name)
                {
                    activeTasks.Add(t);
                    break;
                }
            }
        }
        foreach (var name in completedNames)
        {
            foreach (var t in allTasks)
            {
                if (t.taskName == name)
                {
                    completedTasks.Add(t);
                    break;
                }
            }
        }
    }

    public void CheckTaskProgress()
    {
        for (int i = activeTasks.Count - 1; i >= 0; i--)
        {
            TaskDataSO task = activeTasks[i];
            if (IsTaskCompleted(task))
            {
                CompleteTask(task);
                activeTasks.RemoveAt(i);
            }
        }
    }

    private bool IsTaskCompleted(TaskDataSO task)
    {
        switch (task.taskType)
        {
            case TaskType.BuildBuilding:
                int builtCount = BuildingManager.Instance.GetBuiltCount(task.targetId);
                return builtCount >= task.targetCount;

            case TaskType.HaveResourceAmount:
                int resourceAmount = 0;
                switch (task.targetId)
                {
                    case "Wood": resourceAmount = ResourceManager.Instance.Wood; break;
                    case "Stone": resourceAmount = ResourceManager.Instance.Stone; break;
                    case "Silver": resourceAmount = ResourceManager.Instance.Silver; break;
                    case "Happiness": resourceAmount = ResourceManager.Instance.Happiness; break;
                    case "TechPoints": resourceAmount = ResourceManager.Instance.TechPoints; break;
                }
                return resourceAmount >= task.targetCount;

            case TaskType.UnlockTech:
                return TechManager.Instance.IsTechUnlocked(task.targetId);

            default:
                return false;
        }
    }

    private void CompleteTask(TaskDataSO task)
    {
        // 发放奖励
        ResourceManager.Instance.AddTechPoints(task.rewardTechPoints);
        ResourceManager.Instance.AddResources(task.rewardSilver, task.rewardWood, task.rewardStone);
        ResourceManager.Instance.AddHappiness(task.rewardHappiness);

        completedTasks.Add(task);
        Debug.Log($"任务完成：{task.taskName}，获得奖励");
        OnTaskCompleted?.Invoke(task);
    }

    // 事件响应
    private void OnTechUnlocked(TechNodeData tech)
    {
        CheckTaskProgress();
    }
}