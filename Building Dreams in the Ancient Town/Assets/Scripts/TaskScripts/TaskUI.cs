using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TaskUI : MonoBehaviour
{
    public Transform taskListParent;          // 放置任务条目的父物体
    public GameObject taskItemPrefab;         // 任务条目预制体

    private Dictionary<TaskDataSO, GameObject> taskItems = new Dictionary<TaskDataSO, GameObject>();

    private void OnEnable()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskProgress += UpdateTaskUI;
            TaskManager.Instance.OnTaskCompleted += RemoveTaskUI;
            RefreshAllTasks();
        }
    }

    private void OnDisable()
    {
        if (TaskManager.Instance != null)
        {
            TaskManager.Instance.OnTaskProgress -= UpdateTaskUI;
            TaskManager.Instance.OnTaskCompleted -= RemoveTaskUI;
        }
    }

    private void RefreshAllTasks()
    {
        foreach (Transform child in taskListParent)
            Destroy(child.gameObject);
        taskItems.Clear();

        foreach (var task in TaskManager.Instance.activeTasks)
        {
            CreateTaskItem(task);
        }
    }

    private void CreateTaskItem(TaskDataSO task)
    {
        GameObject item = Instantiate(taskItemPrefab, taskListParent);
        // 假设预制体中有两个 TextMeshProUGUI：名称和描述
        TextMeshProUGUI nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = item.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null) nameText.text = task.taskName;
        if (descText != null) descText.text = task.description;

        taskItems[task] = item;
    }

    private void UpdateTaskUI(TaskDataSO task)
    {
        // 可更新进度，目前简化
    }

    private void RemoveTaskUI(TaskDataSO task)
    {
        if (taskItems.TryGetValue(task, out GameObject item))
        {
            Destroy(item);
            taskItems.Remove(task);
        }
    }
}