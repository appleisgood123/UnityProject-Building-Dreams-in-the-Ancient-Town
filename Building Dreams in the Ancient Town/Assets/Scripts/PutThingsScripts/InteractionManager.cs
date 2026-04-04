using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance;

    public BuildingInteraction currentInteractable { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetCurrentInteractable(BuildingInteraction interactable)
    {
        // 如果新建筑与当前不同，先取消旧的高亮
        if (currentInteractable != null && currentInteractable != interactable)
        {
            currentInteractable.HidePrompt();
        }
        currentInteractable = interactable;
    }

    public void ClearCurrentInteractable(BuildingInteraction interactable)
    {
        if (currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }

    public void TryInteract()
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
}