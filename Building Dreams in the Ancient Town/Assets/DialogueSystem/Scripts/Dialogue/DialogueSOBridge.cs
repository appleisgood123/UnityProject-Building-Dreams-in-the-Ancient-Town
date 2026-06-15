
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//dialoguesSO获取事件的桥梁，因为SO不能直接调用场景中的事件，这样就可以在场景加载时将事件和对话绑定在一起
public class DialogueSOBridge : MonoBehaviour
{
    [Header("DialogueSO列表")]
    public List<Dialogues> dialogues = new List<Dialogues>();

    [Header("事件列表")]
    public List<UnityEvent> events = new List<UnityEvent>();

    private void Start()
    {
        for (int i = 0; i < dialogues.Count; i++)
        {
            if (dialogues[i]!=null && events[i] != null)
            {
                dialogues[i].curEvent = events[i];
            }
        }
    }
}
