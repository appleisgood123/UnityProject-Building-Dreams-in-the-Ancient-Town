using UnityEngine;

[CreateAssetMenu(fileName = "NewTask", menuName = "Game/Task Data")]
public class TaskDataSO : ScriptableObject
{
    public string taskName;
    [TextArea] public string description;

    // 任务目标类型
    public TaskType taskType;
    // 目标参数：例如需要建造的建筑名称，或需要达到的资源名称
    public string targetId;      // 建筑名称、资源名称等
    public int targetCount;      // 需要达到的数量

    // 奖励
    public int rewardTechPoints;
    public int rewardWood;
    public int rewardStone;
    public int rewardSilver;
    public int rewardHappiness;
}

public enum TaskType
{
    BuildBuilding,          // 建造指定建筑（targetId为建筑名称，targetCount为数量）
    HaveResourceAmount,     // 拥有指定资源量（targetId为资源名如"Wood"，targetCount为数量）
    UnlockTech,             // 解锁指定科技（targetId为科技名称）
    // 可根据需要扩展更多类型
}